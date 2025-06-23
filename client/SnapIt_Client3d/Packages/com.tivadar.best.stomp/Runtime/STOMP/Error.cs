using Best.STOMP.Frames;

using System.Collections.Generic;
using System.Text;

namespace Best.STOMP
{
    /// <summary>
    /// Possible sources of an error in the STOMP client.
    /// </summary>
    public enum ErrorSources
    {
        /// <summary>
        /// Error originated from the server.
        /// </summary>
        Server,

        /// <summary>
        /// Error originated from the client.
        /// </summary>
        Client,

        /// <summary>
        /// Error originated from the transport layer.
        /// </summary>
        Transport
    }

    /// <summary>
    /// Represents an error that occurred within the STOMP client, including details about the source and nature of the error.
    /// </summary>
    public sealed class Error
    {
        /// <summary>
        /// Gets the source of the error, indicating whether it originated from the server, client, or transport layer.
        /// </summary>
        public ErrorSources Source { get; internal set; }

        /// <summary>
        /// Gets the message describing the error.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Gets the STOMP frame associated with the error, if the error originated from the server.
        /// </summary>
        public IncomingFrame Frame { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the Error class with a specified source and message.
        /// </summary>
        /// <param name="source">The source of the error.</param>
        /// <param name="message">The message describing the error.</param>
        internal Error(ErrorSources source, string message)
        {
            this.Source = source;
            this.Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the Error class based on an incoming STOMP frame, typically used when the error originates from the server.
        /// </summary>
        /// <param name="frame">The incoming STOMP frame associated with the error.</param>
        internal Error(IncomingFrame frame)
        {
            this.Source = ErrorSources.Server;
            this.Frame = frame;

            var msg = frame.Headers?.GetValueOrDefault("message", null);
            // Try to convert frame's content if it's has a text/ mimetype
            var contentType = frame.Headers?.GetValueOrDefault("content-type", null);
            if (contentType == null || !contentType.StartsWith("text/"))
                this.Message = msg;
            else
                this.Message = $"{msg}: {Encoding.UTF8.GetString(frame.Body.Data, frame.Body.Offset, frame.Body.Count)}";
        }

        public override string ToString() => $"[Error {this.Source}, \"{this.Message}\", {this.Frame}]";
    }
}
