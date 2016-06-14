using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Extant.Logging
{
    /// <summary>
    /// Thread-safe class for saving debugging logs.
    /// </summary>
    public class DebugLogger : IDebugLogger
    {
        public static event LoggerLoggedHandler AnyLogged;
        private static object _anyLogged_lock = new object();

        /////////////////////////

        public event LoggerLoggedHandler Logged;
        public event LoggerLoggedHandler MessageLogged;
        public event LoggerLoggedHandler WarningLogged;
        public event LoggerLoggedHandler ErrorLogged;

        private String _sourceName;
        private bool _isPostingToConsole = false;
        private IDebugLogger _linkedLog = null;

        private List<LogItem> _log = new List<LogItem>();
        private object _log_lock = new object();

        public DebugLogger(String sourceName, IDebugLogger link = null)
        {
            this._sourceName = sourceName;
            this._linkedLog = link;
        }

        ~DebugLogger()
        {
            MessageLogged = null;
            WarningLogged = null;
            ErrorLogged = null;
        }

        public void LogItem(LogItem li)
        {
            lock (_log_lock)
            {
                _log.Add(li);
            }

            switch (li.LogType)
            {
                case (Logging.LogItem.ItemType.Message):
                    if (MessageLogged != null)
                        MessageLogged(li.ToString());
                    break;

                case (Logging.LogItem.ItemType.Warning):
                    if (WarningLogged != null)
                        WarningLogged(li.ToString());
                    break;

                case (Logging.LogItem.ItemType.Error):
                    if (ErrorLogged != null)
                        ErrorLogged(li.ToString());
                    break;
            }

            if (Logged != null)
                Logged(li.ToString());

            if (_linkedLog != null)
                _linkedLog.LogItem(new LogItem(li, _linkedLog));

            lock (_anyLogged_lock)
            {
                var eventFull = AnyLogged;
                if (eventFull != null)
                    eventFull.Invoke(li.ToString());
            }
        }

        public void LogMessage(string s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            LogItem(new LogItem(DateTime.Now, _sourceName, Logging.LogItem.ItemType.Message, s));
        }

        public void LogWarning(string s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            LogItem(new LogItem(DateTime.Now, _sourceName, Logging.LogItem.ItemType.Warning, s));
        }

        public void LogError(string s)
        {
            if (String.IsNullOrEmpty(s))
                return;

            LogItem(new LogItem(DateTime.Now, _sourceName, Logging.LogItem.ItemType.Error, s));
        }

        public LogItem[] GetLines(int numLines)
        {
            lock (_log_lock)
            {
                if (numLines <= 0)
                    return new LogItem[0];

                if (numLines > _log.Count)
                    numLines = _log.Count;

                return _log.Skip(_log.Count - numLines).Take(numLines).ToArray();
            }
        }

        public IDebugLogger LinkedLogger
        {
            get
            {
                return _linkedLog;
            }
            set
            {
                _linkedLog = value;
            }
        }

        public bool IsPostingToConsole
        {
            get
            {
                return _isPostingToConsole;
            }
            set
            {
                if (_isPostingToConsole)
                    Logged -= Console.WriteLine;

                if (value)
                    Logged += Console.WriteLine;

                _isPostingToConsole = value;
            }
        }

        public string SourceName
        {
            get
            {
                return _sourceName;
            }
        }
    }
}
