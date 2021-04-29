// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
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
        private Semaphore? _clientsSemaphore;
        private readonly IServerLog? _log;
        private bool _canRun;
        private readonly HttpServerConfiguration _configuration;
        private readonly List<IRequestHandler> _handlers;

        /// <summary>
        /// Predicate that can be used to impelement whitelist or blaclist
        /// based on TcpClient information
        /// </summary>
        public Predicate<TcpClient>? ClientFilteringPredicate { get; set; }

        /// <summary>
        /// Creates a new instance of HttpServer
        /// </summary>
        /// <param name="configuration">Server configuration</param>
        /// <param name="log">logger to use</param>
        public HttpServer(HttpServerConfiguration configuration, IServerLog? log = null)
        {
            ConfigurationValidator.ValidateAndTrhowExceptions(configuration);

            _configuration = configuration;
            _log = log;
            _handlers = new List<IRequestHandler>();
            _clientsSemaphore = new Semaphore(0, _configuration.MaxClients);
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
            if (_clientsSemaphore != null)
            {
                _clientsSemaphore.Dispose();
                _clientsSemaphore = null;
            }
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
                    TcpClient client = await _listner.AcceptTcpClientAsync();

                    if (ClientFilteringPredicate != null)
                    {
                        _log?.Info("Running {0} ...", nameof(ClientFilteringPredicate));
                        bool canHandle = ClientFilteringPredicate?.Invoke(client) ?? false;
                        if (!canHandle)
                        {
                            _log?.Warning("{0} returned false. Refusing client connection", nameof(ClientFilteringPredicate));
                            client?.Dispose();
                        }
                    }
                   
                    if (_clientsSemaphore?.WaitOne(100) == true)
                    {
                        await HandleClient(client!);
                        _clientsSemaphore.Release();
                    }
                    else
                    {
                        client?.Dispose();
                        _log?.Warning("Maximum number of clients ({0}) reached. Refusing connection", _configuration.MaxClients);
                    }
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
                        _log?.Warning(serverException.Message);
                        await ServeErrorHandler(response, serverException.ResponseCode);

                    }
                    else
                    {
                        _log?.Critical(ex);
                        await ServeInternalServerError(response, ex);
                    }
                }
            }
        }

        private async Task ServeInternalServerError(HttpResponse response, Exception ex)
        {
            HtmlBuilder builder = new HtmlBuilder("Internal server error");
            builder.AppendParagraph("Internal server error happened");
            if (_configuration.DebugMode)
            {
                builder.AppendHr();
                builder.AppendParagraph($"Message: {ex.Message}");
                builder.AppendParagraph("Stack trace:");
                builder.AppendPre(ex.StackTrace ?? "");
            }
            await response.Write(builder.ToString());
        }

        private async Task ServeErrorHandler(HttpResponse response, HttpResponseCode responseCode)
        {
            response.ResponseCode = responseCode;
            response.ContentType = "text/html";
            if (_configuration.CustomErrorHandlers.ContainsKey(responseCode))
            {
                await response.Write(_configuration.CustomErrorHandlers[responseCode]);
            }
            else
            {
                HtmlBuilder builder = new HtmlBuilder(responseCode.ToString());
                builder.AppendHeader(1, $"{(int)responseCode} - {responseCode}");
                builder.AppendHr();
                builder.AppendParagraph($"More info about this issue <a href=\"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{(int)responseCode}\">can be found here</a>");
                await response.Write(builder.ToString());
            }
        }
    }
}
