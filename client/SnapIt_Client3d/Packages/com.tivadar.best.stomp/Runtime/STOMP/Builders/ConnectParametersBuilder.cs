using Best.HTTP.Request.Authentication;

using System;
using System.Collections.Generic;

namespace Best.STOMP.Builders
{
    /// <summary>
    /// Provides a builder for creating and configuring <see cref="ConnectParameters"/>.
    /// </summary>
    public struct ConnectParametersBuilder
    {
        private ConnectParameters _parameters;

        /// <summary>
        /// Sets the host name for the connection.
        /// </summary>
        /// <param name="host">The host name or IP address of the broker.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithHost(string host)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Host = host;
            return this;
        }

        /// <summary>
        /// Sets the host name and port number for the connection.
        /// </summary>
        /// <param name="host">The host name or IP address of the broker.</param>
        /// <param name="port">The port number where the broker is listening.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithHost(string host, int port)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Host = host;
            this._parameters.Port = port;
            return this;
        }

        /// <summary>
        /// Sets the port number for the connection.
        /// </summary>
        /// <param name="port">The port number where the broker is listening.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithPort(int port)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Port = port;
            return this;
        }

        /// <summary>
        /// Enables the use of TLS for the connection to open a secure communication channel between the client and broker.
        /// </summary>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithTLS(bool useTLS = true)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.UseTLS = useTLS;
            return this;
        }

        /// <summary>
        /// Sets the transport protocol for the connection.
        /// </summary>
        /// <param name="transport">The transport protocol to use.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithTransport(SupportedTransports transport)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Transport = transport;
            return this;
        }

        /// <summary>
        /// Sets the path for WebSocket connections.
        /// </summary>
        /// <param name="path">The path for WebSocket connections.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithPath(string path)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Path = path;
            return this;
        }

        /// <summary>
        /// Sets the virtual host for the connection.
        /// </summary>
        /// <param name="vHost">The virtual host name.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithVirtualHost(string vHost)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.VirtualHost = vHost;
            return this;
        }

        /// <summary>
        /// Sets the credentials for authentication with the broker.
        /// </summary>
        /// <param name="credentials">The credentials for authentication.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithCredentials(Credentials credentials)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Credentials = credentials;
            return this;
        }

        /// <summary>
        /// Sets the heartbeat preferences for the connection.
        /// </summary>
        /// <param name="preferredOutgoing">The preferred interval for outgoing heartbeats.</param>
        /// <param name="preferedIncoming">The preferred interval for incoming heartbeats.</param>
        /// <param name="timeout">The timeout for broker heartbeats.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithHeartBeat(TimeSpan preferredOutgoing, TimeSpan preferedIncoming, TimeSpan timeout)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.PreferredOutgoingHeartbeats = preferredOutgoing;
            this._parameters.PreferredIncomingHeartbeats = preferedIncoming;
            this._parameters.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Adds a custom header to the connection parameters.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        /// <returns>The current builder instance.</returns>
        public ConnectParametersBuilder WithHeader(string name, string value)
        {
            this._parameters = this._parameters ?? new ConnectParameters();
            this._parameters.Headers = this._parameters.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this._parameters.Headers.Add(name, value);
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="ConnectParameters"/> instance.
        /// </summary>
        /// <returns>The configured <see cref="ConnectParameters"/> instance.</returns>
        public ConnectParameters Build()
        {
            if (this._parameters.Credentials != null)
            {
                if (!string.IsNullOrEmpty(this._parameters.Credentials.UserName))
                    WithHeader("login", this._parameters.Credentials.UserName);

                if (!string.IsNullOrEmpty(this._parameters.Credentials.Password))
                    WithHeader("passcode", this._parameters.Credentials.Password);
            }

            return this._parameters;
        }
    }
}
