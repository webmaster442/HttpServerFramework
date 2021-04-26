using System;

namespace Webmaster442.HttpServer
{
    /// <summary>
    /// Logging interface for the server
    /// </summary>
    public interface IServerLog
    {
        /// <summary>
        /// Log a critical exception.
        /// </summary>
        /// <param name="ex">Exception to log</param>
        void Critical(Exception ex);

        /// <summary>
        /// Log an iformational message.
        /// Informations give the user feedback about what is happening.
        /// </summary>
        /// <param name="format">Message, a fomat string that can be handled by the String.Format method</param>
        /// <param name="args">Arguments for formatting</param>
        void Info(string format, params object[] args);
    }
}
