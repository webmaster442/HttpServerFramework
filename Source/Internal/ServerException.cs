// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using Webmaster442.HttpServer.Domain;

namespace Webmaster442.HttpServer
{
    /// <summary>
    /// Represents a server exception
    /// </summary>
    [Serializable]
    internal class ServerException: Exception
    {
        public HttpResponseCode ResponseCode { get; }

        /// <summary>
        /// Creates a new instance of server exception
        /// </summary>
        /// <param name="responseCode">Response code describing the issue</param>
        public ServerException(HttpResponseCode responseCode) : base($"{(int)responseCode}: {responseCode}")
        {
            ResponseCode = responseCode;
        }
    }
}
