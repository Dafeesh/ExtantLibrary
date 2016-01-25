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

        public SimpleTimer(TimeSpan startTime = default(TimeSpan))
        {
            this._offset = startTime;
        }

        public static SimpleTimer StartNew(TimeSpan startTime = default(TimeSpan))
        {
            SimpleTimer newTimer = new SimpleTimer(startTime);
            newTimer.Start();

            return newTimer;
        }

        public new void Restart(TimeSpan startTime = default(TimeSpan))
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
