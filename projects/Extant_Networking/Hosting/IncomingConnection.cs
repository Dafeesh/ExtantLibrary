using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Extant.Net.Hosting
{
    public class IncomingConnection
    {
        private const long TIMEOUT_TIME = 5000;

        public TcpConnection TcpConnection { get; private set; }
        public UInt32 Token { get; private set; }

        private Stopwatch _timeoutTimer;

        public IncomingConnection(TcpConnection tcpCon, UInt32 token)
        {
            this.TcpConnection = tcpCon;
            this.Token = token;
            this._timeoutTimer = Stopwatch.StartNew();
        }

        public bool IsTimedOut
        {
            get
            {
                return _timeoutTimer.ElapsedMilliseconds > TIMEOUT_TIME;
            }
        }
    }
}
