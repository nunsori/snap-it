using Best.HTTP.Shared.PlatformSupport.Memory;

using System.Collections.Generic;

namespace Best.STOMP
{
    /// <summary>
    /// Types of frames that can be received from a STOMP server.
    /// </summary>
    public enum ServerFrameTypes
    {
        /// <summary>
        /// Indicates a frame sent by the server to acknowledge a successful connection.
        /// </summary>
        Connected,

        /// <summary>
        /// Represents a message frame containing data sent from a destination.
        /// </summary>
        Message,

        /// <summary>
        /// Indicates a receipt frame sent by the server in response to a client's request that required acknowledgment.
        /// </summary>
        Receipt,

        /// <summary>
        /// Represents an error frame sent by the server in response to an erroneous client action or message.
        /// </summary>
        Error,

        /// <summary>
        /// Represents a ping frame used for maintaining the connection alive.
        /// </summary>
        Ping
    }

    /// <summary>
    /// Represents an incoming frame from the STOMP server.
    /// Contains details about the frame type, headers, body, content type, and other relevant information.
    /// </summary>
    public sealed class IncomingFrame
    {
        /// <summary>
        /// Gets the type of the frame.
        /// </summary>
        public ServerFrameTypes Type { get; internal set; }

        /// <summary>
        /// Gets the headers associated with the frame.
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }

        /// <summary>
        /// Gets the body of the frame as a buffer segment.
        /// </summary>
        public BufferSegment Body { get; internal set; }

        /// <summary>
        /// Gets the content type of the frame.
        /// </summary>
        public string ContentType { get; internal set; }

        /// <summary>
        /// Gets the receipt identifier associated with the frame, if any.
        /// </summary>
        public string Receipt { get; internal set; }

        /// <summary>
        /// Gets the content length of the frame.
        /// </summary>
        internal int ContentLength { get; set; } = -1;

        internal IncomingFrame() { }

        public override string ToString()
        {
            return $"[{nameof(IncomingFrame)} {Type}, {Headers?.Count}, {ContentType}, {Receipt}, {Body}]";
        }
    }
}
