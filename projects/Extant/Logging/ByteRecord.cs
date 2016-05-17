using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Extant.Util;

namespace Extant.Logging
{
    /// <summary>
    /// Used to record byte usage and returns analytics about the data.
    /// </summary>
    public class ByteRecord
    {
        private SimpleTimer elapsed;
        private int bytes;
        private int bytes_last; //Used to signify the last record of total bytes to calculate BPS.
        private float kiloBytesPerSecond;

        public ByteRecord(int b = 0)
        {
            elapsed = SimpleTimer.StartNew();

            bytes = b;
            bytes_last = bytes;
            kiloBytesPerSecond = 0;
        }

        public int Bytes
        {
            set
            {
                bytes = value;
            }

            get
            {
                return bytes;
            }
        }

        public float KiloBytesPerSecond
        {
            get
            {
                if (elapsed.ElapsedMilliseconds > 1000)
                {
                    kiloBytesPerSecond = ((bytes - bytes_last) / 1024.0f) / (elapsed.ElapsedMilliseconds * 1000.0f);

                    bytes_last = bytes;
                    elapsed.Reset();
                    elapsed.Start();
                }
                return kiloBytesPerSecond;
            }
        }
    }
}
