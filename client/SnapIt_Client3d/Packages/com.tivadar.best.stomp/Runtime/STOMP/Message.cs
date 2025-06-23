using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.HTTP.Shared;
using Best.STOMP.Frames;

using System;
using System.Collections.Generic;

namespace Best.STOMP
{
    /// <summary>
    /// Represents a message received from the broker, containing details about the subscription, headers, and content.
    /// </summary>
    public readonly struct Message
    {
        /// <summary>
        /// Gets the <see cref="Best.STOMP.Subscription">subscription</see> associated with this message.
        /// </summary>
        public readonly Subscription Subscription;

        /// <summary>
        /// Gets the message Id set by the broker.
        /// </summary>
        public readonly string MessageId;

        /// <summary>
        /// Unique ID set by the broker that the client will send back as while performing a <see cref="SendACK(Transaction)"/> or <see cref="SendNACK(Transaction)"/> call.
        /// </summary>
        /// <remarks>
        /// If the message is received from a subscription that requires explicit acknowledgment (either client or client-individual mode) then the MESSAGE frame MUST also contain an ack header with an arbitrary value. 
        /// This header will be used to relate the message to a subsequent ACK or NACK frame.
        /// </remarks>
        public readonly string ACK;

        /// <summary>
        /// Gets the content type header set by the original sender.
        /// </summary>
        public readonly string ContentType;

        /// <summary>
        /// Gets the content bytes sent by the original sender.
        /// </summary>
        public readonly BufferSegment Content;

        /// <summary>
        /// Gets all the headers received from the broker.
        /// </summary>
        public readonly Dictionary<string, string> Headers;

        /// <summary>
        /// Initializes a new instance of the Message struct.
        /// </summary>
        /// <param name="subscription">The subscription associated with the message.</param>
        /// <param name="frame">The incoming frame representing the message.</param>
        internal Message(Subscription subscription, IncomingFrame frame)
        {
            this.Subscription = subscription;

            this.MessageId = frame.Headers.GetValueOrDefault("message-id", string.Empty);

            this.ACK = frame.Headers.GetValueOrDefault("ack", null);
            this.ContentType = frame.ContentType;
            this.Content = frame.Body;
            this.Headers = frame.Headers;
        }

        /// <summary>
        /// Send back to the broker an acknowledgment(ACK) frame to acknowledge that the message is received and processed.
        /// </summary>
        /// <remarks>It can accept a <see cref="Transaction"/> instance if it's in a scope of a transaction.</remarks>
        /// <param name="transaction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SendACK(Transaction transaction)
        {
            if (HTTPManager.Logger.IsDiagnostic)
                HTTPManager.Logger.Verbose(nameof(Message), $"{nameof(SendACK)}({this.ACK}, {transaction})", this.Subscription.Client.Context);

            if (string.IsNullOrEmpty(this.ACK))
                throw new ArgumentNullException(nameof(this.ACK));

            this.Subscription.Client.SendFrame(ClientFrameHelper.CreateACKFrame(this.ACK, transaction, this.Subscription.Client.Context));
        }

        /// <summary>
        /// Send back to the broker a non-acknowledgment(NACK) frame if the message couldn't process it for any reason.
        /// </summary>
        /// <remarks>It can accept a <see cref="Transaction"/> instance if it's in a scope of a transaction.</remarks>
        /// <param name="transaction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SendNACK(Transaction transaction)
        {
            if (HTTPManager.Logger.IsDiagnostic)
                HTTPManager.Logger.Verbose(nameof(Message), $"{nameof(SendNACK)}({this.ACK}, {transaction})", this.Subscription.Client.Context);

            if (string.IsNullOrEmpty(this.ACK))
                throw new ArgumentNullException(nameof(this.ACK));

            this.Subscription.Client.SendFrame(ClientFrameHelper.CreateNACKFrame(this.ACK, transaction, this.Subscription.Client.Context));
        }

        public override string ToString()
            => $"[Message '{this.Subscription.Destination}', MId: '{this.MessageId}', '{this.ContentType}', '{this.ACK}', {this.Content}]";
    }
}
