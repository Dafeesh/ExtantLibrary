using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Extant.Threading;
using Extant.Logging;

namespace Extant.Net.Host
{
    public class NetTcpHost : ThreadRun, INetHost, IDebugLogging
    {
        private TcpListener _tcpListener;
        private IPEndPoint _localEndPoint;

        private LockValuePair<List<INetConnection>> _activeConnections = new LockValuePair<List<INetConnection>>(new List<INetConnection>());
        private LockValuePair<Queue<INetConnection>> _newConnections = new LockValuePair<Queue<INetConnection>>(new Queue<INetConnection>());

        private Type _inboundPacketGroup;
        private Type _outboundPacketGroup;

        private DebugLogger _log;

        public NetTcpHost(IPEndPoint localEndPoint, Type inboundPacketGroup, Type outboundPacketGroup, IDebugLogger parentLogger = null)
            : base("TcpHost:" + (localEndPoint != null ? localEndPoint.Port.ToString() : "NULL"))
        {
            if (localEndPoint == null || inboundPacketGroup == null || outboundPacketGroup == null)
                throw new ArgumentNullException("Packet group and local endpoint must not be null.");

            this._tcpListener = new TcpListener(localEndPoint);
            this._localEndPoint = localEndPoint;
            this._inboundPacketGroup = inboundPacketGroup;
            this._outboundPacketGroup = outboundPacketGroup;

            this._log = new DebugLogger("TcpHost{" + localEndPoint.Port + "}", parentLogger);

            this.RegisterTickCall(Tick, TimeSpan.FromMilliseconds(250));
        }

        protected override void OnBegin()
        {
            _tcpListener.Start();
            Log.LogMessage("Started.");
        }

        private void Tick()
        {
            AcceptNewClients();
            CheckClientsForDisconnectOrTimeout();
        }

        protected override void OnFinish(Exception unhandledException = null)
        {
            try
            {
                _tcpListener.Stop();
            }
            catch { }

            lock (_activeConnections.Lock)
            {
                _activeConnections.Value.RemoveAll((c) =>
                {
                    c.Close();
                    return true;
                });
            }

            if (unhandledException == null)
                Log.LogMessage("Finished.");
            else
                Log.LogError("Error while running: " + unhandledException.ToString());
        }

        //////////////////////

        /// <summary>
        /// Returns a pending connection or null if none exist.
        /// </summary>
        public INetConnection PollNewConnection()
        {
            lock (_newConnections.Lock)
            {
                while (_newConnections.Value.Count > 0 && !_newConnections.Value.Peek().IsActive)
                    _newConnections.Value.Dequeue().Close(NetConnectionClosingReason.ClosedSelf);

                if (_newConnections.Value.Count > 0)
                    return _newConnections.Value.Dequeue();
                else
                    return null;
            }
        }

        public void Close()
        {
            this.Stop(false);
        }

        //////////////////////

        private void AcceptNewClients()
        {
            while (_tcpListener.Pending())
            {
                TcpClient newClient = _tcpListener.AcceptTcpClient();
                INetConnection newConnection = new NetStreamConnection(
                    newClient.GetStream(),
                    _inboundPacketGroup, _outboundPacketGroup,
                    (IPEndPoint)newClient.Client.LocalEndPoint,
                    (IPEndPoint)newClient.Client.RemoteEndPoint);
                lock (_activeConnections.Lock)
                    _activeConnections.Value.Add(newConnection);
                lock (_newConnections.Lock)
                    _newConnections.Value.Enqueue(newConnection);
                Log.LogMessage("Client added: " + newConnection.RemoteEndPoint.ToString());
            }
        }

        private void CheckClientsForDisconnectOrTimeout()
        {
            lock (_activeConnections.Lock)
            {
                _activeConnections.Value.RemoveAll((ac) =>
                {
                    if (!ac.IsActive)
                    {
                        Log.LogMessage("Client removed: " + ac.RemoteEndPoint);
                        ac.Close();
                        return true;
                    }
                    else
                        return false;
                });
            }
        }

        //////////////////////

        public int ActiveConnectionsCount
        {
            get
            {
                lock (_activeConnections.Lock)
                {
                    return _activeConnections.Value.Count;
                }
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return _localEndPoint;
            }
        }

        public IDebugLogger Log
        {
            get
            {
                return _log;
            }
        }
    }
}
