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
                        await ServeErrorHandler(response, serverException.ResponseCode);

                    }
                    else
                    {
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
