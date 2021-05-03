// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using Webmaster442.HttpServerFramework.Domain;

namespace Webmaster442.HttpServerFramework
{
    /// <summary>
    /// Represents a server exception
    /// </summary>
    [Serializable]
    internal class ServerException: Exception
    {
        public HttpResponseCode ResponseCode { get; }
        public string Url { get; }

        /// <summary>
        /// Creates a new instance of server exception
        /// </summary>
        /// <param name="responseCode">Response code describing the issue</param>
        public ServerException(HttpResponseCode responseCode) 
            : base($"{(int)responseCode}: {responseCode}")
        {
            ResponseCode = responseCode;
        }

        /// <summary>
        /// Creates a new instance of server exception
        /// </summary>
        /// <param name="responseCode">Response code describing the issue</param>
        /// <param name="url">Invoker url</param>
        public ServerException(HttpResponseCode responseCode, string url)
            : base($"{(int)responseCode}: {responseCode} - {url}")
        {
            ResponseCode = responseCode;
            Url = url;
        }
    }
}
