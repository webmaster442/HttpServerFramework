// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Webmaster442.HttpServer.Domain;
using Webmaster442.HttpServer.Internal;

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
        private readonly List<IRequestHandler> _handlers;


        /// <summary>
        /// Creates a new instance of HttpServer
        /// </summary>
        /// <param name="configuration">Server configuration</param>
        /// <param name="log">logger to use</param>
        public HttpServer(HttpServerConfiguration configuration, IServerLog? log = null)
        {
            _configuration = configuration;
            _log = log;
            _handlers = new List<IRequestHandler>();
        }

        /// <summary>
        /// Add a handler that can process requests to the server
        /// </summary>
        /// <param name="handler">Request handler</param>
        public void RegisterHandler(IRequestHandler handler)
        {
            _handlers.Add(handler);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            _listner = null;
            foreach (var handler in _handlers)
            {
                if (handler is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts listening for connections and serving them
        /// </summary>
#pragma warning disable S3168 // "async" methods should not return "void"
        public async void Start()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            if (_handlers.Count < 1)
                throw new InvalidOperationException("No request handlers are configured");

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
            var parser = new RequestParser(_configuration.MaxPostSize);
            using (client)
            {
                using var stream = client.GetStream();
                HttpResponse response = new HttpResponse(stream);
                try
                {
                    HttpRequest request = parser.ParseRequest(stream);

                    bool wasHandled = false;
                    foreach (var handler in _handlers)
                    {
                        bool result = await handler.Handle(_log, request, response);
                        if (result)
                        {
                            wasHandled = true;
                            break;
                        }
                    }

                    if (!wasHandled)
                    {
                        throw new ServerException(HttpResponseCode.NotFound);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ServerException serverException)
                    {
                        //Handle it correctly
                        ServeErrorHandler(response);

                    }
                    else
                    {
                        ServeInternalServerError(response);
                        //internal server error
                    }
                }
            }
        }

        private void ServeInternalServerError(HttpResponse response)
        {
            throw new NotImplementedException();
        }

        private void ServeErrorHandler(HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
