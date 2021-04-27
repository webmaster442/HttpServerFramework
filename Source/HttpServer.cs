// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Webmaster442.HttpServer
{
    /// <summary>
    /// A Http server
    /// </summary>
    public sealed class HttpServer : IDisposable
    {
        private TcpListener? _listner;
        private readonly IServerLog? _log;
        private bool _canRun;
        private readonly HttpServerConfiguration _configuration;


        /// <summary>
        /// Creates a new instance of HttpServer
        /// </summary>
        /// <param name="configuration">Server configuration</param>
        /// <param name="log">logger to use</param>
        public HttpServer(HttpServerConfiguration configuration, IServerLog? log = null)
        {
            _configuration = configuration;
            _log = log;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            _listner = null;
        }

        /// <summary>
        /// Starts listening for connections and serving them
        /// </summary>
#pragma warning disable S3168 // "async" methods should not return "void"
        public async void Start()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            _listner?.Start();
            while (_canRun && _listner != null)
            {
                try
                {
                    var client = await _listner.AcceptTcpClientAsync();
                    await HandleClient(client);
                }
                catch (Exception ex)
                {
                    _log?.Critical(ex);
                }
            }
        }


        /// <summary>
        /// Stops listening for connections and serving them
        /// </summary>
        public void Stop()
        {
            _canRun = false;
            _listner?.Stop();
        }

        private async Task HandleClient(TcpClient client)
        {
            await Task.Yield();
            using (client)
            {
                using (var stream = client.GetStream())
                {
                    try
                    {

                    }
                    catch (ServerException ex)
                    {
                        
                    }
                }
            }
        }
    }
}
