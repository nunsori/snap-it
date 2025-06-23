using Best.STOMP.Frames;

using System;
using System.Collections.Generic;

namespace Best.STOMP
{
    /// <summary>
    /// Represents the server parameters negotiated after a successful connection to a STOMP broker.
    /// Includes details such as session ID, STOMP protocol version, server information, and heartbeat preferences.
    /// </summary>
    public sealed class ServerParameters
    {
        /// <summary>
        /// A session identifier that uniquely identifies the client's session.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Version of the negotiated STOMP protocol.
        /// </summary>
        public readonly string Version;

        /// <summary>
        /// A field that contains information about the STOMP server. The field MUST contain a server-name field and MAY be followed by optional comment fields delimited by a space octet.
        /// </summary>
        /// <remarks>The server-name field consists of a name token followed by an optional version number token.</remarks>
        public readonly string Server;

        /// <summary>
        /// Heart-beating can optionally be used to test the healthiness of the underlying TCP connection and to make sure that the remote end is alive and kicking.
        /// </summary>
        /// <remarks>
        /// In order to enable heart-beating, each party has to declare what it can do and what it would like the other party to do. 
        /// This happens at the very beginning of the STOMP session, by adding a heart-beat header to the CONNECT and CONNECTED frames.
        /// represents what the broker can do (outgoing heart-beats):
        /// <list type="bullet">
        ///     <item><description><c>0</c> means it cannot send heart-beats</description></item>
        ///     <item><description>otherwise it is the smallest number of milliseconds between heart-beats that it can guarantee</description></item>
        /// </list>
        /// </remarks>
        public readonly TimeSpan PreferredOutgoingHeartbeats;

        /// <summary>
        /// Heart-beating can optionally be used to test the healthiness of the underlying TCP connection and to make sure that the remote end is alive and kicking.
        /// </summary>
        /// <remarks>
        /// In order to enable heart-beating, each party has to declare what it can do and what it would like the other party to do. 
        /// This happens at the very beginning of the STOMP session, by adding a heart-beat header to the CONNECT and CONNECTED frames.
        /// Represents what the broker would like to get(incoming heart-beats):
        /// <list type="bullet">
        ///     <item><description><c>0</c> means it does not want to receive heart-beats</description></item>
        ///     <item><description>otherwise it is the desired number of milliseconds between heart-beats</description></item>
        /// </list>
        /// </remarks>
        public readonly TimeSpan PreferredIncomingHeartbeats;

        /// <summary>
        /// All headers received from the server.
        /// </summary>
        public readonly Dictionary<string, string> Headers;

        internal ServerParameters(IncomingFrame frame)
        {
            this.Headers = frame.Headers;

            if (frame.Headers.TryGetValue("session", out var session))
                this.Id = session;

            if (frame.Headers.TryGetValue("version", out var version))
                this.Version = version;

            if (frame.Headers.TryGetValue("heart-beat", out var heartBeat))
            {
                var beats = heartBeat.Split(',');
                this.PreferredOutgoingHeartbeats = TimeSpan.FromMilliseconds(int.Parse(beats[0]));
                this.PreferredIncomingHeartbeats = TimeSpan.FromMilliseconds(int.Parse(beats[1]));
            }

            if (frame.Headers.TryGetValue("server", out var server))
                this.Server = server;
        }
    }
}
