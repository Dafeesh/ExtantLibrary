using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Logging
{
    public delegate void LoggerLoggedHandler(string str);

    public interface IDebugLogger
    {
        event LoggerLoggedHandler Logged;
        event LoggerLoggedHandler MessageLogged;
        event LoggerLoggedHandler WarningLogged;
        event LoggerLoggedHandler ErrorLogged;

        void LogItem(LogItem item);
        void LogMessage(string str);
        void LogWarning(string str);
        void LogError(string str);

        LogItem[] GetLines(int numLines);

        bool IsPostingToConsole { get; set; }
        string SourceName { get; }
        IDebugLogger LinkedLogger { get; set; }
    }
}
