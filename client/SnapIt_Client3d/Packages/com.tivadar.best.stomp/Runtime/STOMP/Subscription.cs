using Best.HTTP.Shared;
using Best.STOMP.Frames;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Best.STOMP
{
    /// <summary>
    /// Represents possible modes to acknowledge broker sent messages.
    /// </summary>
    public enum AcknowledgmentModes
    {
        /// <summary>
        /// When the ack mode is auto, then the client does not need to send the server ACK frames for the messages it receives. 
        /// The server will assume the client has received the message as soon as it sends it to the client. 
        /// This acknowledgment mode can cause messages being transmitted to the client to get dropped.
        /// </summary>
        Auto,

        /// <summary>
        /// When the ack mode is client, then the client MUST send the server ACK frames for the messages it processes. 
        /// If the connection fails before a client sends an ACK frame for the message the server will assume the message has not been processed and MAY redeliver the message to another client. 
        /// The ACK frames sent by the client will be treated as a cumulative acknowledgment.
        /// This means the acknowledgment operates on the message specified in the ACK frame and all messages sent to the subscription before the ACK'ed message.
        /// </summary>
        /// <remarks>In case the client did not process some messages, it SHOULD send NACK frames to tell the server it did not consume these messages.</remarks>
        Client,

        /// <summary>
        /// When the ack mode is client-individual, the acknowledgment operates just like the client acknowledgment mode except that the ACK or NACK frames sent by the client are not cumulative. 
        /// This means that an ACK or NACK frame for a subsequent message MUST NOT cause a previous message to get acknowledged.
        /// </summary>
        ClientIndividual
    }

    /// <summary>
    /// Represents a subscription to a destination in the STOMP broker.
    /// </summary>
    public sealed class Subscription
    {
        /// <summary>
        /// Gets the client associated with this subscription.
        /// </summary>
        public Client Client { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the subscription.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the destination to which this subscription is subscribed.
        /// </summary>
        public string Destination { get; private set; }

        /// <summary>
        /// Gets the acknowledgment mode for this subscription.
        /// </summary>
        public AcknowledgmentModes AcknowledgmentMode { get; private set; }

        private List<Action<Client, Subscription, Message>> _callbacks;

        internal Subscription(Client parent, int id, string destination, AcknowledgmentModes acknowledgmentMode)
        {
            Client = parent;
            Id = id;
            Destination = destination;
            AcknowledgmentMode = acknowledgmentMode;
        }

        /// <summary>
        /// Adds a callback to be invoked when a message is received on this subscription.
        /// </summary>
        /// <param name="callback">The callback to add.</param>
        public void Add(Action<Client, Subscription, Message> callback)
        {
            HTTPManager.Logger.Verbose(nameof(Subscription), $"{nameof(Add)}()", this.Client.Context);

            if (this._callbacks == null)
                this._callbacks = new List<Action<Client, Subscription, Message>>();

            this._callbacks.Add(callback);
        }

        /// <summary>
        /// Clears all callbacks associated with this subscription.
        /// </summary>
        public void Clear()
        {
            HTTPManager.Logger.Verbose(nameof(Subscription), $"{nameof(Clear)}()", this.Client.Context);

            this._callbacks?.Clear();
        }

        /// <summary>
        /// Removes a specific callback from this subscription.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        public void Remove(Action<Client, Subscription, Message> callback)
        {
            HTTPManager.Logger.Verbose(nameof(Subscription), $"{nameof(Remove)}()", this.Client.Context);

            this._callbacks?.Remove(callback);
        }

        /// <summary>
        /// Begins the process of unsubscribing from the destination and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <param name="acknowledgmentCallback">An optional callback to be invoked upon acknowledgment of unsubscription.</param>
        public void BeginUnsubscribe(Action<Client, Subscription> acknowledgmentCallback)
        {
            HTTPManager.Logger.Verbose(nameof(Subscription), $"{nameof(BeginUnsubscribe)}()", this.Client.Context);

            // Remove the subscription only when a receipt is received acknowledging its removal.
            // When the callback is received, we can call acknowledgmentCallback too, if there's any.
            var receiptId = this.Client.ReceiptManager.SubsribeTo((session, frame) =>
            {
                session.RemoveSubscription(this);

                try
                {
                    acknowledgmentCallback?.Invoke(session, this);
                }
                catch(Exception ex)
                {
                    HTTPManager.Logger.Exception(nameof(Subscription), nameof(acknowledgmentCallback), ex, this.Client.Context);
                }
            });

            this.Client.SendFrame(ClientFrameHelper.CreateUnsubscribeFrame(receiptId, this, null, this.Client.Context));
        }

        /// <summary>
        /// Begins the process of unsubscribing from the destination and optionally executes a callback upon acknowledgment.
        /// </summary>
        /// <returns>A task representing the asynchronous unsubscription operation.</returns>
        public Task<Subscription> UnsubscribeAsync()
        {
            var tcs = new TaskCompletionSource<Subscription>();
            this.BeginUnsubscribe((client, sub) => tcs.TrySetResult(sub));
            return tcs.Task;
        }

        internal bool OnFrame(IncomingFrame frame)
        {
            bool handled = false;

            if (this._callbacks != null)
            {
                var message = new Message(this, frame);
                foreach (var callback in this._callbacks)
                {
                    try
                    {
                        callback?.Invoke(this.Client, this, message);
                        handled = true;
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(Subscription), $"Exception in Subscription callback!", ex, this.Client.Context);
                    }
                }
            }

            return handled;
        }

        public override string ToString() => $"[{nameof(Subscription)} {this.Id}, '{this.Destination}', {this.AcknowledgmentMode}]";
    }
}
