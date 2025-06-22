using Best.HTTP.Shared;
using Best.HTTP.Shared.Extensions;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.STOMP.Frames;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Best.STOMP.Builders
{
    /// <summary>
    /// Provides an interface for building and sending messages to a STOMP broker.
    /// </summary>
    public struct MessageBuilder
    {
        private Client _client;
        private string _destination;
        private string _contentType;
        private Dictionary<string, string> _headers;
        private Action<Client, IncomingFrame> _ackCallback;

        private BufferSegment _content;
        private bool _fromPool;

        private Transaction _transaction;

        internal MessageBuilder(Client client, string destination)
        {
            this._client = client;
            this._destination = destination;
            this._contentType = null;
            this._headers = null;
            this._ackCallback = null;
            this._content = BufferSegment.Empty;
            this._fromPool = false;
            this._transaction = null;
        }

        /// <summary>
        /// Sets the content type for the message.
        /// </summary>
        /// <param name="contentType">The content type of the message.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithContentType(string contentType)
        {
            this._contentType = contentType;
            return this;
        }

        /// <summary>
        /// Adds a custom header to the message.
        /// </summary>
        /// <param name="header">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithHeader(string  header, string value)
        {
            this._headers = this._headers ?? new Dictionary<string, string>();
            this._headers.Add(header, value);

            return this;
        }

        /// <summary>
        /// Sets a callback to be invoked upon acknowledgment of the message.
        /// </summary>
        /// <remarks>When the message is sent in scope of a transaction, the callback will be called ONLY when the transaction is committed!</remarks>
        /// <param name="ackCallback">The acknowledgment callback.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithAcknowledgmentCallback(Action<Client, IncomingFrame> ackCallback)
        {
            this._ackCallback = ackCallback;
            return this;
        }

        /// <summary>
        /// Sets the content of the message as a string.
        /// </summary>
        /// <remarks>It also sets the <c>Content-Type</c> header to <c>text/plain;charset=utf-8</c>! If a different <c>Content-Type</c> should be set, call <see cref="WithContentType(string)"/> after this call.</remarks>
        /// <param name="content">The string content of the message.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithContent(string content)
        {
            int requiredBytes = Encoding.UTF8.GetByteCount(content);
            var buffer = BufferPool.Get(requiredBytes, true, this._client.Context);
            Encoding.UTF8.GetBytes(content, 0, content.Length, buffer, 0);

            this._content = buffer.AsBuffer(requiredBytes);
            this._contentType = "text/plain;charset=utf-8";
            this._fromPool = true;

            return this;
        }

        /// <summary>
        /// Sets the content of the message as a byte array.
        /// </summary>
        /// <param name="content">The byte array content of the message.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithContent(byte[] content)
        {
            this._content = content.AsBuffer();
            return this;
        }

        /// <summary>
        /// Sets the content of the message as a byte array with specified offset and count.
        /// </summary>
        /// <param name="content">The byte array content of the message.</param>
        /// <param name="offset">The offset in the byte array at which to begin.</param>
        /// <param name="count">The number of bytes to use.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithContent(byte[] content, int offset, int count)
        {
            this._content = content.AsBuffer(offset, count);
            return this;
        }

        /// <summary>
        /// Sets the content of the message using a <see cref="BufferSegment"/>.
        /// </summary>
        /// <param name="buffer">The buffer segment representing the message content.</param>
        /// <param name="isFromPool">Indicates whether the buffer is from a memory pool.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithContent(BufferSegment buffer, bool isFromPool)
        {
            this._content = buffer;
            this._fromPool = isFromPool;
            return this;
        }

        /// <summary>
        /// Associates the message with a <see cref="Transaction">transaction</see>.
        /// </summary>
        /// <param name="transaction">The <see cref="Transaction">transaction</see> to associate with the message.</param>
        /// <returns>The current builder instance.</returns>
        public MessageBuilder WithTransaction(Transaction transaction)
        {
            this._transaction = transaction;
            return this;
        }

        /// <summary>
        /// Begins sending the message to the broker.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if essential properties like client or destination are not set.</exception>
        public void BeginSend()
        {
            if (this._client == null)
                throw new ArgumentNullException(nameof(this._client));
            if (this._destination == null)
                throw new ArgumentNullException(nameof(this._destination));

            long receiptId = 0;
            if (this._ackCallback != null)
            {
                var ack = this._ackCallback;

                receiptId = this._client.ReceiptManager.SubsribeTo((client, frame) =>
                {
                    try
                    {
                        ack?.Invoke(client, frame);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception(nameof(MessageBuilder), $"ACK Callback", ex, client.Context);
                    }
                });
            }

            var frame = ClientFrameHelper.CreateSendFrame(receiptId, this._destination, this._contentType, this._transaction, this._headers, this._content, this._client.Context);
            if (this._fromPool)
                BufferPool.Release(this._content);

            this._client.SendFrame(frame);
        }

        /// <summary>
        /// Begins sending the message to the broker.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if essential properties like client or destination are not set.</exception>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public Task<(Client, IncomingFrame)> SendAsync()
        {
            var tcs = new TaskCompletionSource<(Client, IncomingFrame)>();

            this.WithAcknowledgmentCallback((client, frame) => tcs.TrySetResult((client, frame)));
            this.BeginSend();

            return tcs.Task;
        }
    }
}
