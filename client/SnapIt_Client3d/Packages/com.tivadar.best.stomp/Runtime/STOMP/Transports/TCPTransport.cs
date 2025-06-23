#if !UNITY_WEBGL || UNITY_EDITOR
using Best.HTTP.HostSetting;
using Best.HTTP.Shared;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared.PlatformSupport.Network.Tcp;
using Best.HTTP.Shared.Streams;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Best.STOMP.Transports
{
    internal sealed class TCPTransport : Transport, INegotiationPeer, IContentConsumer
    {
        private Negotiator _negotiator;

        public PeekableContentProviderStream ContentProvider { get; private set; }

        private bool _closed;
        private int _senderThreadCreated = 0;
        private AutoResetEvent _are = new AutoResetEvent(false);

        public TCPTransport(Client parent)
            :base(parent)
        {

        }

        public override void BeginConnect(CancellationToken token)
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(BeginConnect)}({token})", this.Context);

            ChangeStateTo(TransportStates.Connecting, null);

            HTTPManager.Setup();
            base.BeginConnect(token);

            NegotiationParameters parameters = new NegotiationParameters();
            parameters.context = this.Context;

            parameters.proxy = HTTPManager.Proxy;
            parameters.createProxyTunel = true;
            parameters.targetUri = new Uri($"tcp://{this.Parent.Parameters.Host}:{this.Parent.Parameters.Port}");
            parameters.negotiateTLS = this.Parent.Parameters.UseTLS;
            parameters.token = token;

            parameters.hostSettings = HTTPManager.PerHostSettings.Get(HostKey.From(parameters.targetUri, new Best.HTTP.Request.Settings.ProxySettings() { Proxy = parameters.proxy }));

            this._negotiator = new Negotiator(this, parameters);
            this._negotiator.Start();
        }

        public override void BeginDisconnect()
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(BeginDisconnect)}()", this.Context);

            base.BeginDisconnect();

            if (this.State >= TransportStates.Disconnecting)
                return;

            ChangeStateTo(TransportStates.Disconnecting, string.Empty);
            try
            {
                this._closed = true;
                this._are.Set();
            }
            catch
            {
                ChangeStateTo(TransportStates.Disconnected, string.Empty);
            }
        }

        public override void Send(BufferSegment frameBytes)
        {
            if (this.State == TransportStates.Disconnected)
                return;

            if (System.Threading.Interlocked.CompareExchange(ref this._senderThreadCreated, 1, 0) == 0)
                Best.HTTP.Shared.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(SendThread);

            this._outgoingFrames.Enqueue(frameBytes);
            this._are?.Set();
        }

        private void SendThread()
        {
            try
            {
                while (!this._closed)
                {
                    this._are.WaitOne();
                    while (this._outgoingFrames.TryDequeue(out var frame))
                        this._negotiator.Streamer.EnqueueToSend(frame);
                }
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception(nameof(TCPTransport), nameof(SendThread), ex, this.Context);
            }
            finally
            {
                this._negotiator?.Stream?.Close();
                //this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Disconnected));
            }
        }

        protected override void CleanupAfterDisconnect()
        {
            base.CleanupAfterDisconnect();

            this._are.Dispose();
            this._are = null;
        }

        List<string> INegotiationPeer.GetSupportedProtocolNames(Negotiator negotiator) => new List<string> { Best.HTTP.Hosts.Connections.HTTPProtocolFactory.W3C_HTTP1 };

        bool INegotiationPeer.MustStopAdvancingToNextStep(Negotiator negotiator, NegotiationSteps finishedStep, NegotiationSteps nextStep, Exception error)
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(INegotiationPeer.MustStopAdvancingToNextStep)}({negotiator}, {finishedStep}, {nextStep}, {error})", this.Context);

            bool stop = negotiator.Parameters.token.IsCancellationRequested || error != null;

            if (stop)
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error?.ToString() ?? "IsCancellationRequested"));

            return stop;
        }

        void INegotiationPeer.EvaluateProxyNegotiationFailure(Negotiator negotiator, Exception error, bool resendForAuthentication)
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(INegotiationPeer.EvaluateProxyNegotiationFailure)}({negotiator}, {error}, {resendForAuthentication})", this.Context);

            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error?.ToString() ?? "Proxy authentication failed"));
        }

        void INegotiationPeer.OnNegotiationFailed(Negotiator negotiator, Exception error)
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(INegotiationPeer.OnNegotiationFailed)}({negotiator}, {error})", this.Context);

            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, error.ToString()));
        }

        void INegotiationPeer.OnNegotiationFinished(Negotiator negotiator, PeekableContentProviderStream stream, TCPStreamer streamer, string negotiatedProtocol)
        {
            HTTPManager.Logger.Verbose(nameof(TCPTransport), $"{nameof(INegotiationPeer.OnNegotiationFinished)}({negotiator}, {stream}, {streamer}, {negotiatedProtocol})", this.Context);

            stream.SetTwoWayBinding(this);

            this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Connected));
        }

        void IContentConsumer.SetBinding(PeekableContentProviderStream contentProvider) => this.ContentProvider = contentProvider;
        void IContentConsumer.UnsetBinding() => this.ContentProvider = null;

        void IContentConsumer.OnContent()
        {
            var available = this.ContentProvider.Length;
            var buffer = BufferPool.Get(available, true, this.Context);
            int count = this.ContentProvider.Read(buffer, 0, (int)available);

            this._receiveStream.Write(new BufferSegment(buffer, 0, count));

            HTTPManager.Logger.Information(nameof(TCPTransport), $"{nameof(IContentConsumer.OnContent)} - Received ({count})", this.Context);

            try
            {
                TryParseIncomingFrames();
            }
            catch (Exception ex)
            {
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.Exception, ex, nameof(TryParseIncomingFrames)));
            }
        }

        void IContentConsumer.OnConnectionClosed() 
            => this.transportEvents.Enqueue(
                State == TransportStates.Disconnecting ? 
                    new TransportEvent(TransportEventTypes.StateChange, TransportStates.Disconnected) :
                    new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, "TCP closed by remote peer."));

        void IContentConsumer.OnError(Exception ex)
            => this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.DisconnectedWithError, ex.Message));
    }
}
#endif