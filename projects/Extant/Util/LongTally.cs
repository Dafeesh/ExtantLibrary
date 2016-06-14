using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Util
{
    public interface ILongTally
    {
        long Total { get; }
        long PerSecond { get; }
    }

    public class LongTally : ILongTally
    {
        private long _total;
        private long _perSecond = 0;

        private long _previousTotal;
        private SimpleTimer _secondsTimer = new SimpleTimer();

        public LongTally(long startingTotal = 0)
        {
            this.Restart(startingTotal);
        }

        public void Restart(long startingTotal = 0)
        {
            if (startingTotal < 0)
                throw new ArgumentException("Starting total seconds cannot be negative.");

            this._total = this._previousTotal = startingTotal;

            _secondsTimer.Restart(TimeSpan.FromMilliseconds(startingTotal));
        }

        public long Add(long amount)
        {
            if (amount < 0)
                throw new ArgumentException("Cannot tally a negative value: " + amount);

            return (_total += amount);
        }

        public long Total
        {
            get
            {
                return _total;
            }
        }

        public long PerSecond
        {
            get
            {
                if (_secondsTimer.ElapsedMilliseconds > 1000)
                {
                    _perSecond = (_total - _previousTotal) / (_secondsTimer.ElapsedMilliseconds / 1000); //Long automatically rounds to 'floor' value
                    _previousTotal = _total;
                    _secondsTimer.Restart(TimeSpan.FromMilliseconds(_secondsTimer.ElapsedMilliseconds % 1000));
                }

                return _perSecond;
            }
        }
    }
}
