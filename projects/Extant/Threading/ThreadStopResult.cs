using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Threading
{
    public enum ThreadStopType
    {
        Success,
        HandledFailure,
        UnhandledException
    }

    public enum ThreadStopSource
    {
        Self,
        Parent,
        External,
        GarbageCollection
    }

    public class ThreadStopResult
    {
        public ThreadStopType Type { get; set; }
        public ThreadStopSource Source { get; set; }

        public ThreadStopResult(ThreadStopType type, ThreadStopSource source)
        {
            this.Type = type;
            this.Source = source;
        }
    }
}
