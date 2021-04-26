﻿using System;
using System.Collections.Generic;

namespace Webmaster442.HttpServer.Domain
{
    /// <summary>
    /// Represents a HTTP Request header
    /// </summary>
    public record HttpRequest
    {
        /// <summary>
        /// Request method
        /// </summary>
        public RequestMethod Method { get; init; }
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; init; }
        /// <summary>
        /// Request Arguments
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; init; }
        /// <summary>
        /// Request version string.
        /// </summary>
        public string Version { get; init; }
        /// <summary>
        /// Request size in bytes
        /// </summary>
        public int RequestSize { get; init; }

        /// <summary>
        /// Request content
        /// </summary>
        public byte[] RequestContent { get; init; }

        /// <summary>
        /// Additional headers
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; init; }

        /// <summary>
        /// Creates a new instance of HttpRequest
        /// </summary>
        public HttpRequest()
        {
            Url = string.Empty;
            Version = string.Empty;
            RequestSize = 0;
            Headers = new Dictionary<string, string>();
            Parameters = new Dictionary<string, string>();
            RequestContent = Array.Empty<byte>();
        }

    }
}