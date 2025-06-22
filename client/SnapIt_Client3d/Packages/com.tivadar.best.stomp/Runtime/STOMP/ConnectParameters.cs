using Best.HTTP.Request.Authentication;

using System;
using System.Collections.Generic;

namespace Best.STOMP
{
    /// <summary>
    /// Supported transport protocols for connecting to a STOMP broker.
    /// </summary>
    public enum SupportedTransports
    {
        /// <summary>
        /// Represents a raw TCP transport protocol.
        /// </summary>
        TCP,

        /// <summary>
        /// Represents the WebSocket transport protocol.
        /// </summary>
        WebSocket
    }

    /// <summary>
    /// Represents the connection parameters required for establishing a connection with a STOMP broker.
    /// </summary>
    public sealed class ConnectParameters
    {
        /// <summary>
        /// Gets or sets the host name or IP address of the broker.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port number where the broker is listening.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a secure protocol (TLS over TCP; or wss:// for WebSockets).
        /// </summary>
        public bool UseTLS { get; set; }

        /// <summary>
        /// Gets or sets the selected transport protocol to use for connecting.
        /// </summary>
        public SupportedTransports Transport { get; set; }

        /// <summary>
        /// Gets or sets the optional path for WebSocket connections. Default is "/ws".
        /// </summary>
        public string Path { get; set; } = "/ws";

        /// <summary>
        /// The virtual host to use for the connection. If null or empty, Uri.Host will be used.
        /// </summary>
        public string VirtualHost { get; set; } = null;

        /// <summary>
        /// Credentials for authentication, if required by the broker.
        /// </summary>
        public Credentials Credentials { get; set; } = null;

        /// <summary>
        /// Gets or sets the preferred heartbeat interval for outgoing heartbeat messages.
        /// </summary>
        public TimeSpan PreferredOutgoingHeartbeats { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the preferred heartbeat interval for incoming heartbeat messages.
        /// </summary>
        public TimeSpan PreferredIncomingHeartbeats { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the timeout for broker heartbeats.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Additional headers to send with the first, connect packet.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = null;
    }
}