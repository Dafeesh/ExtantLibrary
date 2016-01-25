using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Logging
{
    public delegate void LoggerLoggedHandler(string str);

    public interface ILogger
    {
        event LoggerLoggedHandler Logged;
        event LoggerLoggedHandler MessageLogged;
        event LoggerLoggedHandler WarningLogged;
        event LoggerLoggedHandler ErrorLogged;

        void LogMessage(string str);
        void LogWarning(string str);
        void LogError(string str);

        LogItem[] GetLines(int numLines);

        bool IsPostingToConsole { get; set; }
        string SourceName { get; }
    }
}
