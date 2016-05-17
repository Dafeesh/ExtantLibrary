using System;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Logging
{
    public class LogItem
    {
        public DateTime Time;
        public String Source;
        public ItemType LogType;
        public String Message;

        public LogItem(DateTime time, String source, ItemType logType, String message)
        {
            this.Time = time;
            this.Source = source;
            this.LogType = logType;
            this.Message = message;
        }

        public LogItem(LogItem item, IDebugLogger sourceParent)
            : this(item.Time, item.Source, item.LogType, item.Message)
        {
            this.Source = sourceParent.SourceName + "-" + this.Source;
        }

        public override string ToString()
        {
            string timeStamp = Time.ToString("HH:mm:ss tt");
            switch (this.LogType)
            {
                default: //Normal
                    return timeStamp + "[" + Source + "]: " + Message;

                case (ItemType.Warning):
                    return timeStamp + "<Warning> [" + Source + "]: " + Message;

                case (ItemType.Error):
                    return timeStamp + "<ERROR> [" + Source + "]: " + Message;
            }
        }

        public enum ItemType
        {
            Message,
            Warning,
            Error
        }
    }
}
