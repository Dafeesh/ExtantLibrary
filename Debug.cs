using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Extant
{
    /// <summary>
    /// Thread-safe class for saving debugging logs.
    /// </summary>
    public class DebugLogger
    {
        /////////////
        public static readonly DebugLogger GlobalDebug = new DebugLogger();
        /////////////

        public enum LogType
        {
            Blank,
            Networking,
            System,
            Catch,
            Fatal
        }

        private List<String> log = new List<String>();
        private object thisLock = new object();

        public delegate void DebugLogMessageDelegate(String message);
        public event DebugLogMessageDelegate MessageLogged;

        public DebugLogger()
        {  }

        private void _Log(String title, String s)
        {
            lock (thisLock)
            {
                String m = DateTime.Now.ToString("HH:mm:ss tt");
                if (title != String.Empty)
                    m += "[" + title + "]";
                m += ": " + s;

                log.Add(m);

                if (MessageLogged != null)
                    MessageLogged(m);
            }
        }

        public void SaveLog(string filename)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType t, String s)
        {
            switch(t)
            {
                case(LogType.Blank):
                    _Log(String.Empty, s);
                    break;
                case(LogType.Catch):
                    _Log("Catch", s);
                    break;
                case(LogType.Fatal):
                    _Log("Fatal", s);
                    break;
                case(LogType.Networking):
                    _Log("Net", s);
                    break;
                case(LogType.System):
                    _Log("System", s);
                    break;
                default:
                    _Log("LOGERROR", "Unknown LogType: " + t.ToString() + " - \n" + s + "\n-");
                    break;
            }
        }
    }

    /// <summary>
    /// Used to record byte usage and returns analytics about the data.
    /// </summary>
    public class ByteRecord
    {
        /////////////
        public static readonly ByteRecord GlobalByteRecord_In = new ByteRecord();
        public static readonly ByteRecord GlobalByteRecord_Out = new ByteRecord();
        /////////////

        private Stopwatch elapsed;
        private int bytes;
        private int bytes_last; //Used to signify the last record of total bytes to calculate BPS.
        private float kiloBytesPerSecond;

        public ByteRecord(int b = 0)
        {
            elapsed = new Stopwatch();
            elapsed.Start();

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
