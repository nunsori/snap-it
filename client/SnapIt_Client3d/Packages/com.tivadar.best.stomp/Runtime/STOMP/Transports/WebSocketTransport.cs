using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.WebSockets;

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Best.STOMP.Transports
{
    internal sealed class WebSocketTransport : Transport
    {
        private WebSocket _webSocket;
        public WebSocketTransport(Client parent)
            : base(parent)
        {
        }

        public override void BeginConnect(CancellationToken token)
        {
            HTTPManager.Logger.Verbose(nameof(WebSocketTransport), $"{nameof(BeginConnect)}({token})", this.Context);

            base.BeginConnect(token);

            if (token.IsCancellationRequested)
                return;

            if (this.State != TransportStates.Initial)
                throw new Exception($"{nameof(WebSocketTransport)} couldn't {nameof(BeginConnect)} as it's already in state {this.State}!");

            ChangeStateTo(TransportStates.Connecting, string.Empty);

            var options = this.Parent.Parameters;

            string url = $"{(options.UseTLS ? "wss" : "ws")}://{options.Host}";

            // if (options.Port > 0)
            //     url += $":{options.Port}{(options.Path.StartsWith("/") ? string.Empty : "/")}{options.Path ?? string.Empty}";
            if (options.Port > 0)
            {
                string path = options.Path ?? string.Empty;

                if (path.Contains("?"))
                {
                    var parts = path.Split(new[] { '?' }, 2);
                    var basePath = parts[0];
                    var query = parts[1];

                    url += $":{options.Port}{(basePath.StartsWith("/") ? string.Empty : "/")}{basePath}?{query}";
                }
                else
                {
                    url += $":{options.Port}{(path.StartsWith("/") ? string.Empty : "/")}{path}";
                }
            }

            //sori


            this._webSocket = new WebSocket(new Uri(url), origin: null, protocol: "v12.stomp");
            this._webSocket.Context.Add("Parent", this.Context);
            this._webSocket.OnOpen += WebSocket_OnOpen;
            this._webSocket.OnClosed += WebSocket_OnClosed;
            this._webSocket.OnMessage += WebSocket_OnMessage;
            this._webSocket.OnBinary += WebSocket_OnBinary;
#if !UNITY_WEBGL || UNITY_EDITOR
            this._webSocket.SendPings = false;
#endif
            this._webSocket.Open();
        }

        private void WebSocket_OnOpen(WebSocket webSocket) => this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, TransportStates.Connected));
        private void WebSocket_OnClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
            => this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.StateChange, code == WebSocketStatusCodes.NormalClosure ? TransportStates.Disconnected : TransportStates.DisconnectedWithError, message));

        private void WebSocket_OnMessage(WebSocket webSocket, string message)
        {
            if (this._receiveStream == null)
                return;

            var count = Encoding.UTF8.GetByteCount(message);
            var data = BufferPool.Get(count, true, this.Context);
            Encoding.UTF8.GetBytes(message, 0, message.Length, data, 0);

            var buffer = data.AsBuffer(count);

            if (HTTPManager.Logger.IsDiagnostic)
                HTTPManager.Logger.Information(nameof(WebSocketTransport), $"{nameof(WebSocket_OnMessage)}({message}, {buffer})", this.Context);

            this._receiveStream.Write(buffer);

            try
            {
                TryParseIncomingFrames();
            }
            catch (Exception ex)
            {
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.Exception, ex, nameof(TryParseIncomingFrames)));
            }
        }

        private void WebSocket_OnBinary(WebSocket webSocket, BufferSegment buffer)
        {
            var data = BufferPool.Get(buffer.Count, true, this.Context);
            buffer.CopyTo(data);
            this._receiveStream.Write(data.AsBuffer(buffer.Count));

            try
            {
                TryParseIncomingFrames();
            }
            catch (Exception ex)
            {
                this.transportEvents.Enqueue(new TransportEvent(TransportEventTypes.Exception, ex, nameof(TryParseIncomingFrames)));
            }
        }

        public override void BeginDisconnect()
        {
            if (this.State >= TransportStates.Disconnecting)
                return;

            HTTPManager.Logger.Verbose(nameof(WebSocketTransport), $"{nameof(BeginDisconnect)}()", this.Context);

            base.BeginDisconnect();

            this.ChangeStateTo(TransportStates.Disconnecting, string.Empty);
            this._webSocket.Close();
        }

        public override void Send(BufferSegment frameBytes)
        {
            if (this.State != TransportStates.Connected)
            {
                HTTPManager.Logger.Warning(nameof(WebSocketTransport), $"Send called while it's not in the Connected state! State: {this.State}", this.Context);
                return;
            }

            if (HTTPManager.Logger.IsDiagnostic)
                HTTPManager.Logger.Information(nameof(WebSocketTransport), $"{nameof(Send)}({frameBytes})", this.Context);

            this._webSocket.SendAsBinary(frameBytes);
        }

        protected override void CleanupAfterDisconnect()
        {
            HTTPManager.Logger.Verbose(nameof(WebSocketTransport), $"{nameof(CleanupAfterDisconnect)}()", this.Context);

            base.CleanupAfterDisconnect();

            this._webSocket?.Close();
            this._webSocket = null;
        }
    }
}
