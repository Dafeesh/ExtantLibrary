using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 0109

namespace Extant.Util
{
    public class SimpleTimer : System.Diagnostics.Stopwatch
    {
        private TimeSpan _offset;

        public SimpleTimer()
            : this(TimeSpan.Zero)
        { }

        public SimpleTimer(TimeSpan startTime)
        {
            this._offset = startTime;
        }

        public static new SimpleTimer StartNew()
        {
            return StartNew(TimeSpan.Zero);
        }

        public static SimpleTimer StartNew(TimeSpan startTime)
        {
            SimpleTimer newTimer = new SimpleTimer(startTime);
            newTimer.Start();

            return newTimer;
        }

        public new void Restart()
        {
            Restart(TimeSpan.Zero);
        }

        public new void Restart(TimeSpan startTime)
        {
            this._offset = startTime;
            this.Reset();
            this.Start();
        }

        public new TimeSpan Elapsed
        {
            get
            {
                return (base.Elapsed + _offset);
            }
        }

        public new long ElapsedMilliseconds
        {
            get
            {
                return (long)this.Elapsed.TotalMilliseconds;
            }
        }

        public new long ElapsedTicks
        {
            get
            {
                return (long)this.Elapsed.Ticks;
            }
        }
    }
}
