using Best.HTTP.Shared;
using Best.STOMP.Frames;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Best.STOMP.Builders
{
    /// <summary>
    /// A builder to help in creation of a new <see cref="Subscription"/>.
    /// </summary>
    /// <remarks>A new builder can be created by using the <see cref="Client.CreateSubscriptionBuilder(string)"/> function!</remarks>
    public struct SubscriptionBuilder
    {
        private Client _client;
        private string _destination;
        private AcknowledgmentModes _ackMode;
        private Dictionary<string, string> _headers;
        private Action<Client, Subscription, Message> _callback;
        private Action<Client, Subscription, IncomingFrame> _ackCallback;

        internal SubscriptionBuilder(Client client, string destination)
        {
            this._client = client;
            this._destination = destination;
            this._ackMode = AcknowledgmentModes.Auto;
            this._headers = null;
            this._callback = null;
            this._ackCallback = null;
        }

        /// <summary>
        /// Sets the <see cref="AcknowledgmentModes">acknowledgment mode</see> for the subscription.
        /// </summary>
        /// <param name="ackMode">The <see cref="AcknowledgmentModes">acknowledgment mode</see>.</param>
        /// <returns>The current builder instance.</returns>
        public SubscriptionBuilder WithAcknowledgmentMode(AcknowledgmentModes ackMode)
        {
            this._ackMode = ackMode;
            return this;
        }

        /// <summary>
        /// Sets a callback to be invoked upon acknowledgment of the subscription.
        /// </summary>
        /// <param name="callback">The acknowledgment callback.</param>
        /// <returns>The current builder instance.</returns>
        public SubscriptionBuilder WithAcknowledgmentCallback(Action<Client, Subscription, IncomingFrame> callback)
        {
            this._ackCallback = callback;
            return this;
        }

        /// <summary>
        /// Sets a callback to be invoked when a message is received on this subscription.
        /// </summary>
        /// <param name="callback">The callback to handle incoming messages.</param>
        /// <returns>The current builder instance.</returns>
        public SubscriptionBuilder WithCallback(Action<Client, Subscription, Message> callback)
        {
            this._callback = callback;
            return this;
        }

        /// <summary>
        /// Adds a custom header to the subscription.
        /// </summary>
        /// <param name="header">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <returns>The current builder instance.</returns>
        public SubscriptionBuilder WithHeader(string header, string value)
        {
            this._headers = this._headers ?? new Dictionary<string, string>();
            this._headers.Add(header, value);
            return this;
        }

        /// <summary>
        /// Begins the process of subscribing to the destination.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if essential properties like client or destination are not set.</exception>
        public void BeginSubscribe()
        {
            HTTPManager.Logger.Verbose(nameof(SubscriptionBuilder), $"{nameof(BeginSubscribe)}({this._client}, {this._destination}, {this._ackMode})", this._client.Context);

            if (this._client == null)
                throw new ArgumentException($"{nameof(this._client)} is null! Acquire a {nameof(SubscriptionBuilder)} by calling {nameof(Client)}'s {nameof(Client.CreateSubscriptionBuilder)}!");

            if (string.IsNullOrEmpty(this._destination))
                throw new ArgumentException($"{this._destination} is null! You have to specify a destination while calling {nameof(Client.CreateSubscriptionBuilder)}!");

            var (sub, receiptId) = this._client.AddSubscription(this._destination, this._ackMode, this._ackCallback);

            if (this._callback != null)
                sub.Add(this._callback);

            var frameData = ClientFrameHelper.CreateSubscribeFrame(receiptId, sub, this._headers, this._client.Context);
            this._client.SendFrame(frameData);
        }

        /// <summary>
        /// Begins the process of subscribing to the destination.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if essential properties like client or destination are not set.</exception>
        /// <returns>A task representing the asynchronous subscription operation and the acknowledgment frame.</returns>
        public Task<(Subscription, IncomingFrame)> SubscribeAsync()
        {
            var tcs = new TaskCompletionSource<(Subscription, IncomingFrame)>();
            this.WithAcknowledgmentCallback((client, sub, frame) => tcs.TrySetResult((sub, frame)));
            return tcs.Task;
        }
    }
}
