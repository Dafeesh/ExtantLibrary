using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

using Extant.Util;
using Extant.Threading;
using Extant.Logging;

namespace Extant.Net.Hosting
{
    public class HostingConnection : INetConnection
    {
        private TcpConnection _tcpConnection;
        private LockValuePair<UdpClient> _udpClient;
        private IPEndPoint _remoteUdpEndPoint;

        private Queue<NetPacket> _udpPackets = new Queue<NetPacket>();
        private object _udpPackets_lock = new object();

        private SimpleTimer _lastPacketTimer = SimpleTimer.StartNew();
        DebugLogger _log;

        public HostingConnection(
            TcpConnection tcpCon,
            LockValuePair<UdpClient> udpClient,
            IPEndPoint remoteUdpEndPoint
            )
        {
            this._log = new DebugLogger("HostCon");

            this._tcpConnection = tcpCon;
            this._tcpConnection.Log.LinkedLogger = this._log;
            this._udpClient = udpClient;
            this._remoteUdpEndPoint = remoteUdpEndPoint;
        }

        ////////////////////////////////////////////////

        /// <summary>
        /// Since a Host can have multiple connections from only one UDP endpoint, this makes sure
        ///  each connection receives the right UDP packet.
        /// </summary>
        public void AddUdpPacket(NetPacket p)
        {
            lock (_udpPackets_lock)
            {
                _udpPackets.Enqueue(p);
            }
        }

        /// <summary>
        /// Sends a packet to this connection.
        /// </summary>
        public void Send(NetPacket packet, NetworkProtocol protocol)
        {
            switch (protocol)
            {
                case (NetworkProtocol.TCP):
                    _tcpConnection.SendPacket(packet);
                    break;

                case (NetworkProtocol.UDP):
                    byte[] data = packet.GetBytes();
                    lock (_udpClient.Lock)
                    {
                        _udpClient.Value.Send(data, data.Length, _remoteUdpEndPoint);
                    }
                    break;

                default:
                    throw new NotSupportedException("That protocol is not supported.");
            }
        }

        public NetPacket Receive()
        {
            //TCP
            if (_tcpConnection.PacketAvailable)
            {
                ResetLastPacketTimer();
                return _tcpConnection.GetPacket();
            }

            //UDP
            lock (_udpPackets_lock)
            {
                if (_udpPackets.Count > 0)
                {
                    ResetLastPacketTimer();
                    return _udpPackets.Dequeue();
                }
            }

            //None
            return null;
        }

        public void Close()
        {
            _tcpConnection.Close();
        }

        ////////////////////////////////////////////////

        private void ResetLastPacketTimer()
        {
            _lastPacketTimer.Reset();
            _lastPacketTimer.Start();
        }

        ////////////////////////////////////////////////

        public IPEndPoint RemoteUdpEndPoint
        {
            get
            {
                return _remoteUdpEndPoint;
            }
        }

        public bool IsActive
        {
            get
            {
                return _tcpConnection.IsActive || this.PacketAvailable;
            }
        }

        public bool PacketAvailable
        {
            get
            {
                return _tcpConnection.PacketAvailable || _udpPackets.Count > 0;
            }
        }

        public long TimeSinceLastPacket
        {
            get
            {
                return _lastPacketTimer.ElapsedMilliseconds;
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
