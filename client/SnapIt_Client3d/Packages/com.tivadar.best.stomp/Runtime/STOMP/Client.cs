using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.Logger;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.STOMP.Builders;
using Best.STOMP.Frames;
using Best.STOMP.Transports;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Best.STOMP
{
    /// <summary>
    /// Represents the various states of the STOMP client during its lifecycle.
    /// </summary>
    public enum States
    {
        /// <summary>
        /// Indicates the initial state of the client, before any connection attempt has been made.
        /// </summary>
        Initial,

        /// <summary>
        /// Represents the state where the client is attempting to establish a connection with the STOMP broker using the configured transport.
        /// </summary>
        Connecting,

        /// <summary>
        /// Indicates that the client has successfully connected to the STOMP broker.
        /// </summary>
        Connected,

        /// <summary>
        /// Represents the state where the client is in the process of disconnecting from the STOMP broker.
        /// </summary>
        Disconnecting,

        /// <summary>
        /// Indicates that the client has disconnected from the STOMP broker.
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// Represents a client for communicating with a <see href="https://stomp.github.io">STOMP (Simple Text Oriented Messaging Protocol)</see> v1.2 broker.
    /// </summary>
    /// <remarks>
    /// STOMP is a simple, text-based protocol offering an interoperable wire format that enables clients to communicate with a wide range of message brokers and queueing systems, ensuring broad compatibility across different STOMP message brokers.
    /// This class manages the connection lifecycle, message subscription and publishing, transaction handling, and heartbeating as per the <see href="https://stomp.github.io/stomp-specification-1.2.html">STOMP 1.2</see> specification.
    /// <para>
    /// Key features include:
    /// <list type="bullet">
    /// <item><description>Connecting and disconnecting from a STOMP broker.</description></item>
    /// <item><description>Subscribing to message destinations and receiving messages.</description></item>
    /// <item><description>Sending messages to destinations in the broker.</description></item>
    /// <item><description>Handling message acknowledgments and transactions.</description></item>
    /// <item><description>Maintaining the client state and managing heartbeats to ensure a live connection.</description></item>
    /// </list>
    /// The client supports both callback and async-await based operation patterns, making it suitable for various application scenarios.
    /// </para>
    /// </remarks>

    public sealed class Client : IHeartbeat
    {
        /// <summary>
        /// Gets the parameters used for establishing a connection with the STOMP server.
        /// </summary>
        public ConnectParameters Parameters { get; private set; }

        /// <summary>
        /// Gets the parameters negotiated with the STOMP broker after a successful connection.
        /// </summary>
        public ServerParameters ServerParameters { get; private set; }

        /// <summary>
        /// Current state of the STOMP client. State changed events are emitted through the <see cref="OnStateChanged"/> event.
        /// </summary>
        public States State
        {
            get => this._state;
            private set
            {
                var oldState = this._state;
                if (oldState != value)
                {
                    this._state = value;

                    try
                    {
                        this.OnStateChanged?.Invoke(this, oldState, this._state);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Client), nameof(OnStateChanged), ex, this.Context);
                    }
                }
            }
        }
        private States _state;

        public LoggingContext Context { get; private set; }

        /// <summary>
        /// Called when the client successfully connected to the broker.
        /// </summary>
        public Action<Client, ServerParameters, IncomingFrame> OnConnected;

        /// <summary>
        /// Called after the client disconnects from the broker.
        /// </summary>
        public Action<Client, Error> OnDisconnected;

        /// <summary>
        /// Called for every internal state change of the client.
        /// </summary>
        public Action<Client, States, States> OnStateChanged;

        /// <summary>
        /// Called for every frame received from the broker.
        /// </summary>
        public Func<Client, IncomingFrame, bool> OnFrame;

        /// <summary>
        /// Manager class to wrap receipt Id and callback management.
        /// </summary>
        internal ReceiptManager ReceiptManager { get; private set; }

        /// <summary>
        /// Transport instance that handles the commmunication with the server.
        /// </summary>
        private Transport _transport;

        /// <summary>
        /// Contains the last subscription's id, incremented when a new subscription is created.
        /// </summary>
        private int _subscriptionId;

        /// <summary>
        /// Active subscriptions. The dictionary's key is the unique id assigned by the client.
        /// </summary>
        private Dictionary<int, Subscription> Subsciptions = new Dictionary<int, Subscription>();

        /// <summary>
        /// Contains the last transaction's id, incremented when a new transaction is created.
        /// </summary>
        private int _transactionId = 0;

        /// <summary>
        /// Contains the last time when a client received a frame from the broker.
        /// </summary>
        private DateTime _lastReceivedFrameTime;

        /// <summary>
        /// Contains the last time when the clienbt send a frame to the broker.
        /// </summary>
        private DateTime _lastSentFrameTime;

        /// <summary>
        /// Initializes a new instance of the Client class.
        /// </summary>
        public Client()
        {
            this.ReceiptManager = new ReceiptManager(this);
            this.Context = new LoggingContext(this);
        }

        /// <summary>
        /// Initiates a connection to the STOMP broker using the provided <see cref="ConnectParameters"/>.
        /// </summary>
        /// <param name="parameters">The connection parameters to use.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> for the connect operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
        public void BeginConnect(ConnectParameters parameters, CancellationToken token = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (parameters.Host == null)
                throw new ArgumentNullException(nameof(parameters.Host));

            this.Parameters = parameters;

            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(BeginConnect)}({this.Parameters}, {token})", this.Context);

            switch(this.Parameters.Transport)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                case SupportedTransports.TCP:
                    this._transport = new TCPTransport(this);
                    break;
#endif

                case SupportedTransports.WebSocket:
                    this._transport = new WebSocketTransport(this);
                    break;
                    
                default: throw new NotImplementedException(this.Parameters.Transport.ToString());
            }

            this.State = States.Connecting;
            this._transport.BeginConnect(token);
            HTTPManager.Heartbeats.Subscribe(this);
        }

        /// <summary>
        /// Using the <see cref="ConnectParameters"/> passed as its parameter begins the connection procedure to the broker.
        /// </summary>

        /// <summary>
        /// Asynchronously establishes a connection to the STOMP broker using the provided <see cref="ConnectParameters"/>.
        /// </summary>
        /// <param name="parameters">The connection parameters to use.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> for the connect operation.</param>
        /// <returns>A task representing the asynchronous operation, resulting in the server's response parameters, a <see cref="ServerParameters"/> instance.</returns>
        public Task<ServerParameters> ConnectAsync(ConnectParameters parameters, CancellationToken token = default)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(ConnectAsync)}({this.Parameters}, {token})", this.Context);

            var tcs = new TaskCompletionSource<ServerParameters>();

            this.OnConnected += OnConnectedCallback;
            this.OnDisconnected += OnDisconnectedCallback;

            this.BeginConnect(parameters, token);

            void OnConnectedCallback(Client client, ServerParameters data, IncomingFrame frame)
            {
                this.OnConnected -= OnConnectedCallback;
                this.OnDisconnected -= OnDisconnectedCallback;
                tcs.TrySetResult(data);
            }

            void OnDisconnectedCallback(Client client, Error error)
            {
                this.OnConnected -= OnConnectedCallback;
                this.OnDisconnected -= OnDisconnectedCallback;

                if (error != null)
                    tcs.TrySetException(new Exception(error.Message));
                else
                    tcs.TrySetException(new Exception("Unexpected disconnection!"));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Initiates the disconnection process from the STOMP broker.
        /// </summary>
        public void BeginDisconnect()
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(BeginDisconnect)}({this.State})", this.Context);

            if (this.State >= States.Disconnecting)
                return;

            this.State = States.Disconnecting;

            var receiptId = this.ReceiptManager.SubsribeTo(OnDisconnectAck);

            var frame = ClientFrameHelper.CreateDisconnectFrame(receiptId, null, this.Context);

            this.SendFrame(frame);
        }

        /// <summary>
        /// Asynchronously disconnects from the STOMP broker.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnection operation, resulting in any error that occurred.</returns>
        public Task<Error> DisconnectAsync()
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(DisconnectAsync)}({this.State})", this.Context);

            if (this.State == States.Disconnecting)
                throw new InvalidOperationException($"{nameof(DisconnectAsync)} - Already disconnecting!");

            if (this.State == States.Disconnected)
                throw new InvalidOperationException($"{nameof(DisconnectAsync)} - Already disconnected!");

            var tcs = new TaskCompletionSource<Error>();

            this.OnDisconnected += OnDisconnectedCallback;

            this.BeginDisconnect();

            void OnDisconnectedCallback(Client client, Error error)
            {
                this.OnDisconnected -= OnDisconnectedCallback;
                tcs.TrySetResult(error);
            }

            return tcs.Task;
        }

        private void OnDisconnectAck(Client client, IncomingFrame receiptFrame)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(OnDisconnectAck)}({client}, {receiptFrame}, {this.State})", this.Context);

            this._transport.BeginDisconnect();
        }

        /// <summary>
        /// Creates a new <see cref="Transaction">transaction</see> for sending and receiving messages in a transactional context.
        /// </summary>
        /// <returns>A new <see cref="Transaction"/> instance.</returns>
        public Transaction CreateTransaction()
            => new Transaction(Interlocked.Increment(ref this._transactionId), this);

        /// <summary>
        /// Begins the construction of a new subscription to the specified destination.
        /// </summary>
        /// <param name="destination">The destination to subscribe to.</param>
        /// <returns>A <see cref="SubscriptionBuilder"/> to build and configure the subscription.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null or empty.</exception>
        public SubscriptionBuilder CreateSubscriptionBuilder(string destination)
        {
            if (string.IsNullOrEmpty(destination)) 
                throw new ArgumentNullException(nameof(destination));

            return new SubscriptionBuilder(this, destination);
        }

        /// <summary>
        /// Searches for a subscription by its destination.
        /// </summary>
        /// <param name="destination">The destination of the subscription to find.</param>
        /// <returns>The found <see cref="Subscription"/>, or null if no matching subscription is found.</returns>
        public Subscription FindSubscription(string destination)
            => this.Subsciptions.FirstOrDefault(kvp => kvp.Value.Destination == destination).Value;

        /// <summary>
        /// Begins the construction of a new message to a specified destination.
        /// </summary>
        /// <param name="destination">The destination to send the message to.</param>
        /// <returns>A <see cref="MessageBuilder"/> to build, configure and send the message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="destination"/> is null or empty.</exception>
        public MessageBuilder CreateMessageBuilder(string destination)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));

            return new MessageBuilder(this, destination);
        }

        /// <summary>
        /// Adds a subscription to the client's subscription management.
        /// </summary>
        /// <param name="destination">The destination to subscribe to.</param>
        /// <param name="acknowledgmentMode">The acknowledgment mode for the subscription.</param>
        /// <param name="ackCallback">The callback to be invoked on acknowledgment.</param>
        /// <returns>A tuple containing the subscription and the receipt ID.</returns>
        internal (Subscription subscription, long receiptId) AddSubscription(string destination, AcknowledgmentModes acknowledgmentMode, Action<Client, Subscription, IncomingFrame> ackCallback)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(AddSubscription)}('{destination}')", this.Context);

            var sub = new Subscription(this, Interlocked.Increment(ref this._subscriptionId), destination, acknowledgmentMode);

            this.Subsciptions.Add(sub.Id, sub);

            long receiptId = 0;
            if (ackCallback != null)
                receiptId = this.ReceiptManager.SubsribeTo((client, frame) => ackCallback?.Invoke(client, sub, frame));

            return (sub, receiptId);
        }

        /// <summary>
        /// Removes a subscription from the client's subscription management.
        /// </summary>
        /// <param name="subscription">The subscription to remove.</param>
        internal void RemoveSubscription(Subscription subscription)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(RemoveSubscription)}({subscription})", this.Context);

            this.Subsciptions.Remove(subscription.Id);
        }

        /// <summary>
        /// Sends a STOMP frame to the broker.
        /// </summary>
        /// <param name="frameData">The frame data to be sent.</param>
        internal void SendFrame(BufferSegment frameData)
        {
            if (HTTPManager.Logger.IsDiagnostic)
                HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(SendFrame)}({frameData}, {this.State})", this.Context);

            if (this.State == States.Initial)
                return;

            this._transport.Send(frameData);

            if (HTTPUpdateDelegator.Instance.IsMainThread())
                this._lastSentFrameTime = HTTPManager.CurrentFrameDateTime;
            else
                this._lastSentFrameTime = DateTime.UtcNow;
        }
        
        void IHeartbeat.OnHeartbeatUpdate(DateTime now, TimeSpan dif)
        {
            while (this._transport.IncomingFrames.TryDequeue(out var frame))
            {
                if (HTTPManager.Logger.IsDiagnostic)
                    HTTPManager.Logger.Verbose(nameof(Client), $"Processing Frame {frame.Type}", this.Context);

                this._lastReceivedFrameTime = now;

                if (this.OnFrame != null)
                {
                    try
                    {
                        if (!this.OnFrame.Invoke(this, frame))
                            continue;
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Client), nameof(OnFrame), ex, this.Context);
                    }
                }

                switch(frame.Type)
                {
                    // https://stomp.github.io/stomp-specification-1.2.html#CONNECTED_Frame
                    case ServerFrameTypes.Connected:
                        this.ServerParameters = new ServerParameters(frame);
                        this.State = States.Connected;
                        this.OnConnected?.Invoke(this, this.ServerParameters, frame);
                        break;

                    case ServerFrameTypes.Error:
                        DisconnectedWith(new Error(frame));
                        break;

                    case ServerFrameTypes.Ping:
                        break;

                    case ServerFrameTypes.Receipt:
                        // https://stomp.github.io/stomp-specification-1.2.html#Header_receipt
                        // https://stomp.github.io/stomp-specification-1.2.html#RECEIPT

                        if (frame.Headers.TryGetValue("receipt-id", out var receiptIdStr))
                        {
                            long receiptId = long.Parse(receiptIdStr);
                            this.ReceiptManager.OnReceipt(receiptId, frame);
                        }
                        else
                        {
                            HTTPManager.Logger.Error(nameof(Client), $"No 'receipt-id' header found in RECEIPT frame!", this.Context);
                        }

                        break;

                    case ServerFrameTypes.Message:
                        bool handled = false;
                        AcknowledgmentModes acknowledgmentMode = AcknowledgmentModes.Auto;
                        try
                        {
                            if (frame.Headers.TryGetValue("subscription", out var subscriptionIdStr))
                            {
                                var subscriptionId = int.Parse(subscriptionIdStr);
                                if (this.Subsciptions.TryGetValue(subscriptionId, out var subscription))
                                {
                                    acknowledgmentMode = subscription.AcknowledgmentMode;
                                    handled = subscription.OnFrame(frame);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception(nameof(Client), $"Subscription callback", ex, this.Context);
                        }

                        // https://stomp.github.io/stomp-specification-1.2.html#SUBSCRIBE_ack_Header
                        // When the ack mode is auto, then the client does not need to send the server ACK frames for the messages it receives.
                        if (acknowledgmentMode != AcknowledgmentModes.Auto)
                        {
                            // If the message is received from a subscription that requires explicit acknowledgment (either client or client-individual mode) then the MESSAGE frame MUST also contain an ack header with an arbitrary value.
                            // This header will be used to relate the message to a subsequent ACK or NACK frame.
                            if (!handled)
                                HTTPManager.Logger.Warning(nameof(Client), $"Message({frame}) isn't handled while Subscription({Subsciptions})'s mode is {acknowledgmentMode}. The broker expects an ACK/NACK!", this.Context);
                        }
                        break;
                }
            }

            // Explicitly call transport's OnHeartbeatUpdate after processing incoming frames!
            this._transport?.OnHeartbeatUpdate(now, dif);

            switch (this.State)
            {
                case States.Initial:
                case States.Connecting:
                case States.Disconnecting:
                    break;

                case States.Connected:
                    // https://stomp.github.io/stomp-specification-1.2.html#Heart-beating
                    // client => server pings
                    if (this.Parameters.PreferredOutgoingHeartbeats != TimeSpan.Zero && this.ServerParameters.PreferredIncomingHeartbeats != TimeSpan.Zero)
                    {
                        var expectedHeartbeatTime = Math.Max(this.Parameters.PreferredOutgoingHeartbeats.TotalMilliseconds, this.ServerParameters.PreferredIncomingHeartbeats.TotalMilliseconds);
                        if (now - this._lastSentFrameTime > TimeSpan.FromMilliseconds(expectedHeartbeatTime))
                        {
                            // if the sender has no real STOMP frame to send, it MUST send an end-of-line (EOL)
                            var frameData = BufferPool.Get(1, true, this.Context);
                            frameData[0] = 0x0A;
                            SendFrame(frameData.AsBuffer(1));
                        }
                    }

                    // server => client pings
                    if (this.Parameters.PreferredIncomingHeartbeats != TimeSpan.Zero && this.ServerParameters.PreferredOutgoingHeartbeats != TimeSpan.Zero)
                    {
                        var expectedHeartbeatTime = Math.Max(this.Parameters.PreferredIncomingHeartbeats.TotalMilliseconds, this.ServerParameters.PreferredOutgoingHeartbeats.TotalMilliseconds);
                        if (now - this._lastReceivedFrameTime > (TimeSpan.FromMilliseconds(expectedHeartbeatTime) + this.Parameters.Timeout))
                        {
                            DisconnectedWith(new Error(ErrorSources.Client, $"No frame received in the expected heartbeat time({expectedHeartbeatTime})!"));
                        }
                    }
                    break;

                case States.Disconnected:
                    HTTPManager.Heartbeats.Unsubscribe(this);
                    break;
            }
        }

        internal void TransportConnected()
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(TransportConnected)}()", this.Context);

            this._transport.Send(ClientFrameHelper.CreateSTOMPFrame(this.Parameters, this.Context));
        }

        internal void TransportDisconnectedWithError(string reason)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(TransportDisconnectedWithError)}({reason})", this.Context);

            DisconnectedWith(this.State == States.Disconnecting ? null : new Error(ErrorSources.Transport, reason));
        }

        internal void TransportDisconnected(string reason)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(TransportDisconnected)}({reason})", this.Context);

            DisconnectedWith(this.State == States.Disconnecting ? null : new Error(ErrorSources.Transport, reason));
        }

        internal void TransportError(string source, Exception exception)
        {
            HTTPManager.Logger.Verbose(nameof(Client), $"{nameof(TransportError)}({source}, {exception})", this.Context);
        }

        private void DisconnectedWith(Error error)
        {
            HTTPManager.Logger.Information(nameof(Client), $"{nameof(DisconnectedWith)}({error})", this.Context);

            if (this.State == States.Disconnected)
                return;

            this.State = States.Disconnected;
            try
            {
                this.OnDisconnected?.Invoke(this, error);
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception(nameof(Client), $"{nameof(OnDisconnected)}({error})", ex, this.Context);
            }

            this._transport?.BeginDisconnect();
        }
    }
}
