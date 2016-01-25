using System;
using System.Collections.Generic;
using System.Diagnostics;

using Extant.Extensions;

namespace Extant.Util
{
    public class TimeoutTimer : SimpleTimer
    {
        private TimeSpan _timeoutTime;

        public TimeoutTimer(TimeSpan timeoutTime)
            : base(TimeSpan.Zero)
        {
            this._timeoutTime = timeoutTime;
        }

        public static new TimeoutTimer StartNew(TimeSpan timeoutTime)
        {
            TimeoutTimer newTimer = new TimeoutTimer(timeoutTime);
            newTimer.Start();

            return newTimer;
        }
        
        public new void Restart(TimeSpan newTimeoutTime)
        {
            this._timeoutTime = newTimeoutTime;
            base.Restart();
        }

        public bool IsTimedOut
        {
            get
            {
                return RemainingMilliseconds <= 0;
            }
        }

        public TimeSpan Remaining
        {
            get
            {
                return (_timeoutTime - this.Elapsed).AsPositiveOrDefault();
            }
        }

        public long RemainingMilliseconds
        {
            get
            {
                return (long)Remaining.TotalMilliseconds;
            }
        }
    }
}
