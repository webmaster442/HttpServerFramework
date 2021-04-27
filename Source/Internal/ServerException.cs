// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using Webmaster442.HttpServer.Domain;

namespace Webmaster442.HttpServer
{
    /// <summary>
    /// Represents a server exception
    /// </summary>
    [Serializable]
    internal class ServerException: Exception
    {
        public ServerException()
        {
        }

        /// <summary>
        /// Creates a new instance of server exception
        /// </summary>
        /// <param name="responseCode">Response code describing the issue</param>
        public ServerException(HttpResponseCode responseCode) : base($"{(int)responseCode}: {responseCode}")
        {

        }

        public ServerException(string? message) : base(message)
        {
        }

        public ServerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
