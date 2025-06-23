using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.Logger;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared.Streams;
using Best.STOMP.Frames;

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Best.STOMP.Transports
{
    public enum TransportStates
    {
        Initial,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        DisconnectedWithError
    }

    public enum TransportEventTypes
    {
        StateChange,
        Exception
    }

    public readonly struct TransportEvent
    {
        public readonly TransportEventTypes Type;

        public readonly TransportStates ToState;
        public readonly string Reason;

        public readonly string Source;
        public readonly Exception Exception;

        public TransportEvent(TransportEventTypes type, TransportStates toState)
            : this(type, toState, null)
        {
        }

        public TransportEvent(TransportEventTypes type, TransportStates toState, string reason)
        {
            this.Type = type;
            this.ToState = toState;
            this.Reason = reason;

            this.Source = null;
            this.Exception = null;
        }

        public TransportEvent(TransportEventTypes type, Exception exception, string source)
        {
            this.Type = type;
            this.ToState = TransportStates.Initial;
            this.Reason = null;

            this.Source = source;
            this.Exception = exception;
        }
    }

    internal class Transport : IHeartbeat
    {
        /// <summary>
        /// State of the transport.
        /// </summary>
        public TransportStates State { get; private set; } = TransportStates.Initial;

        /// <summary>
        /// Parent <see cref="Parent"/> of the transport.
        /// </summary>
        public Client Parent { get; private set; }

        /// <summary>
        /// Received and parsed frames, sent by the broker.
        /// </summary>
        public ConcurrentQueue<IncomingFrame> IncomingFrames { get; private set; } = new ConcurrentQueue<IncomingFrame>();

        /// <summary>
        /// Optional <see cref="CancellationToken"/> for connection cancellation support.
        /// </summary>
        public CancellationToken ConnectCancellationToken { get; protected set; } = default;

        /// <summary>
        /// Debug context of the transport.
        /// </summary>
        public LoggingContext Context { get; private set; }

        /// <summary>
        /// Transport event queue generated on receive/send threads that must be processed on the main thread.
        /// </summary>
        protected ConcurrentQueue<TransportEvent> transportEvents = new ConcurrentQueue<TransportEvent>();

        /// <summary>
        /// Intermediate stream holding incomplete frame bytes.
        /// </summary>
        protected PeekableIncomingSegmentStream _receiveStream = new PeekableIncomingSegmentStream();

        protected IncomingFrameFactory _frameFactory;

        protected ConcurrentQueue<BufferSegment> _outgoingFrames = new ConcurrentQueue<BufferSegment>();

        public Transport(Client parent)
        {
            this.Parent = parent;

            this.Context = new LoggingContext(this);
            this.Context.Add("Parent", this.Parent.Context);
        }

        public virtual void BeginConnect(CancellationToken token)
        {
            this.ConnectCancellationToken = token;
        }

        public virtual void BeginDisconnect()
        {

        }

        public virtual void Send(BufferSegment frameBytes)
        {

        }

        internal void TryParseIncomingFrames()
        {
            HTTPManager.Logger.Information(nameof(Transport), $"{nameof(TryParseIncomingFrames)}({this._receiveStream.Length})", this.Context);

            if (this._frameFactory == null)
                this._frameFactory = new IncomingFrameFactory(this.Parent, this._receiveStream);

            while (this._frameFactory.TryAdvance(out var frame))
            {
                if (frame != null)
                    this.IncomingFrames.Enqueue(frame);
            }
        }

        protected void ChangeStateTo(TransportStates newState, string reason)
        {
            HTTPManager.Logger.Information(nameof(Transport), $"{nameof(ChangeStateTo)}({this.State} => {newState}, \"{reason}\")", this.Context);

            if (this.State == newState)
                return;

            var oldState = this.State;
            this.State = newState;

            switch (newState)
            {
                case TransportStates.Connected:
                    if (!this.ConnectCancellationToken.IsCancellationRequested)
                        this.Parent.TransportConnected();
                    break;
                case TransportStates.DisconnectedWithError:
                    if (oldState == TransportStates.Disconnected)
                        break;
                    this.Parent.TransportDisconnectedWithError(reason);
                    this.CleanupAfterDisconnect();
                    break;

                case TransportStates.Disconnected:
                    if (oldState == TransportStates.DisconnectedWithError)
                        break;
                    this.Parent.TransportDisconnected(reason);
                    this.CleanupAfterDisconnect();
                    break;
            }
        }

        public void OnHeartbeatUpdate(DateTime now, TimeSpan dif)
        {
            while (this.transportEvents.TryDequeue(out var result))
            {
                switch (result.Type)
                {
                    case TransportEventTypes.StateChange:
                        ChangeStateTo(result.ToState, reason: result.Reason);
                        break;

                    case TransportEventTypes.Exception:
                        this.Parent.TransportError(result.Source, result.Exception);
                        break;
                }
            }
        }

        protected virtual void CleanupAfterDisconnect()
        {
            HTTPManager.Logger.Information(nameof(Transport), $"{nameof(CleanupAfterDisconnect)}", this.Context);

            this._receiveStream?.Dispose();
            this._receiveStream = null;
            this.IncomingFrames.Clear();
            this._outgoingFrames.Clear();
        }
    }
}
