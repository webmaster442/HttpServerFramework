// ------------------------------------------------------------------------------------------------
// Copyright (c) 2021 Ruzsinszki Gábor
// This is free software under the terms of the MIT License. https://opensource.org/licenses/MIT
// -----------------------------------------------------------------------------------------------

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

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="format">Message, a fomat string that can be handled by the String.Format method</param>
        /// <param name="args">Arguments for formatting</param>
        void Warning(string format, params object[] args);
    }
}
