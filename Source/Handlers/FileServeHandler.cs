// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Webmaster442.HttpServer.Domain;

namespace Webmaster442.HttpServer.Handlers
{
    /// <summary>
    /// Basic file handler.
    /// </summary>
    public class FileServeHandler : IRequestHandler
    {
        private readonly string _path;
        private readonly string _mountPath;

        /// <summary>
        /// List of files that are probed to serve, if the request is a folder;
        /// </summary>
        public string[] IndexFiles { get; }

        /// <summary>
        /// Creates a new instance of FileServeHandler
        /// </summary>
        /// <param name="path">Path in the file system to serve from.</param>
        /// <param name="mountPath">Mount path on server</param>
        public FileServeHandler(string path, string mountPath = "/")
        {
            _path = path;
            _mountPath = mountPath;
            IndexFiles = new [] 
            {
                "index.html",
                "index.htm",
                "default.html",
                "default.htm"
            };
        }

        private string GetIndexFile(string _path)
        {
            foreach (string indexFile in IndexFiles)
            {
                var localindex = Path.Combine(_path, indexFile);
                if (File.Exists(localindex))
                {
                    return localindex;
                }
            }
            throw new ServerException(HttpResponseCode.NotFound);
        }


        /// <inheritdoc/>
        public async Task<bool> Handle(IServerLog? log, HttpRequest request, HttpResponse response)
        {
            if (request.Method != RequestMethod.Get)
            {
                return false;
            }

            if (request.Url.StartsWith(_mountPath))
            {
                string filename = request.Url.Substring(_mountPath.Length);
                var fileOnDisk = Path.Combine(_path, filename);
                if (string.IsNullOrEmpty(filename))
                {
                    fileOnDisk = GetIndexFile(_mountPath);
                }

                if (Directory.Exists(fileOnDisk))
                {
                    fileOnDisk = GetIndexFile(fileOnDisk);
                }

                if (File.Exists(fileOnDisk))
                {
                    using (var stream = File.OpenRead(fileOnDisk))
                    {
                        response.ContentType = MimeTypes.GetMimeTypeForFile(fileOnDisk);
                        await response.Write(stream);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
