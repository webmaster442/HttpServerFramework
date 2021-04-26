using System.Net;

namespace Webmaster442.HttpServer
{
    /// <summary>
    /// Represents the HTTP server configuration
    /// </summary>
    public sealed class HttpServerConfiguration
    {
        /// <summary>
        /// Listen port. If Not changed, the default is 8080
        /// </summary>
        public short Port { get; init; }

        /// <summary>
        /// Listen Adress. Default is Any.
        /// </summary>
        public IPAddress ListenAdress { get; init; }

        /// <summary>
        /// Creates a new Instance of HttpServerConfiguration
        /// </summary>
        public HttpServerConfiguration()
        {
            Port = 8080;
            ListenAdress = IPAddress.Any;
        }
    }
}
