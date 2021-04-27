using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Webmaster442.HttpServer.Domain
{
    /// <summary>
    /// Represents a HTTP Response
    /// </summary>
    public sealed class HttpResponse : IDisposable
    {
        private static byte[] end = Encoding.UTF8.GetBytes("\n\n");

        private NetworkStream? stream;

        /// <summary>
        /// Response code
        /// </summary>
        public HttpResponseCode ResponseCode { get; set; }
        
        /// <summary>
        /// Response content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Additional headers to send
        /// </summary>
        public Dictionary<string, string> AdditionalHeaders { get; }

        /// <summary>
        /// Creates a new instance of HttpResponse
        /// </summary>
        public HttpResponse()
        {
            ContentType = "text/plain";
            AdditionalHeaders = new Dictionary<string, string>();
            ResponseCode = HttpResponseCode.Ok;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        private string PrepareHeaders(long contentLength)
        {
            StringBuilder headers = new StringBuilder();
            headers.Append("HTTP/1.1 ").Append((int)ResponseCode).AppendLine(" ResponseCode");
            headers.Append("Content-Length: ").Append(contentLength).AppendLine();
            headers.Append("Content-Type: ").AppendLine(ContentType);
            foreach (var header in AdditionalHeaders)
            {
                headers.AppendLine($"{header.Key}: {header.Value}");
            }
            headers.AppendLine();
            return headers.ToString();
        }

        /// <summary>
        /// Write text to the client
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <returns>an awaitable task</returns>
        public async Task Write(string text)
        {
            var txt = Encoding.UTF8.GetBytes(text);
            var headers = Encoding.UTF8.GetBytes(PrepareHeaders(txt.Length));
            if (stream != null)
            {
#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
                await stream.WriteAsync(headers);
                await stream.WriteAsync(txt);
                await stream.WriteAsync(end);
#pragma warning restore RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
            }
        }

        /// <summary>
        /// Write binary data to the client
        /// </summary>
        /// <param name="data">a stream containing the data</param>
        /// <returns>an awaitable task</returns>
        public async Task Write(Stream data)
        {
            var headers = Encoding.UTF8.GetBytes(PrepareHeaders(data.Length));
            if (stream != null)
            {
#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
                await stream.WriteAsync(headers);
                await data.CopyToAsync(stream);
                await stream.WriteAsync(end);
#pragma warning restore RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
            }
        }

        /// <summary>
        /// Write binary data to the client
        /// </summary>
        /// <param name="data">data to write</param>
        /// <returns>an awaitable task</returns>
        public async Task Write(byte[] data)
        {
            var headers = Encoding.UTF8.GetBytes(PrepareHeaders(data.Length));
            if (stream != null)
            {
#pragma warning disable RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
                await stream.WriteAsync(headers);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.WriteAsync(end);
#pragma warning restore RCS1090 // Add call to 'ConfigureAwait' (or vice versa).
            }
        }

        /// <summary>
        /// Write a type as JSON string
        /// </summary>
        /// <typeparam name="T">Type parameter</typeparam>
        /// <param name="input">input object</param>
        /// <param name="options">Serializer options</param>
        /// <returns>an awaitable task</returns>
        public async Task WriteJson<T>(T input, JsonSerializerOptions? options = null)
        {
            string serialized = JsonSerializer.Serialize(input, options);
            ContentType = "text/json";
            await Write(serialized);
        }
    }
}
