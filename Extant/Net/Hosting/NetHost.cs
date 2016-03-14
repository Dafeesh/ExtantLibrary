using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Extant.Threading;
using Extant.Logging;

namespace Extant.Net.Hosting
{
    public class NetHost : ThreadRun , IDebugLogging
    {
        private TcpListener _tcpListener;
        private LockValuePair<UdpClient> _udpListener;
        private IPEndPoint _localEndPoint;

        private List<HostingConnection> _activeConnections = new List<HostingConnection>();
        private LockValuePair<Queue<INetConnection>> _newConnections = new LockValuePair<Queue<INetConnection>>(new Queue<INetConnection>());
        private List<IncomingConnection> _incomingConnections = new List<IncomingConnection>();

        private NetPacket.PacketDecryptor _packetDecryptor;
        private Random _tokenRandomizer = new Random();
        private DebugLogger _log;

        public NetHost(NetPacket.PacketDecryptor packetDecryptor, IPEndPoint localEndPoint, DebugLogger parentLogger)
            : base("NetHost")
        {
            if (packetDecryptor == null || localEndPoint == null)
                throw new ArgumentNullException();

            this._packetDecryptor = packetDecryptor;
            this._localEndPoint = localEndPoint;
            this._log = new DebugLogger("NetHost", parentLogger);

            this.RegisterTickCall(Tick, 128);
        }

        protected override void OnBegin()
        {
            _tcpListener = new TcpListener(_localEndPoint);
            _udpListener = new LockValuePair<UdpClient>(new UdpClient(_localEndPoint));

            _tcpListener.Start();
        }

        private void Tick()
        {
            AcceptNewClients();
            CheckClientsForDisconnectOrTimeout();
            ListenForUdp();
        }

        protected override void OnFinish(bool success)
        {
            _tcpListener.Stop();
            lock (_udpListener.Lock)
            {
                _udpListener.Value.Close();
            }

            foreach (var ic in _incomingConnections)
            {
                ic.TcpConnection.Close();
            }
            foreach (var hc in _activeConnections)
            {
                hc.Close();
            }
        }

        //////////////////////

        private void AcceptNewClients()
        {
            while (_tcpListener.Pending())
            {
                TcpClient newTcpClient = _tcpListener.AcceptTcpClient();
                UInt32 token = GenerateRandomToken();
                newTcpClient.Client.Send(BitConverter.GetBytes(token));

                IncomingConnection ic =
                    new IncomingConnection(new TcpConnection(newTcpClient, _packetDecryptor), token);

                _incomingConnections.Add(ic);

                Log.LogMessage("New client!");
            }
        }

        private void CheckClientsForDisconnectOrTimeout()
        {
            //Incoming connections
            foreach (var ic in _incomingConnections.ToArray())
            {
                if (!ic.TcpConnection.IsActive || ic.IsTimedOut)
                {
                    ic.TcpConnection.Close();
                    _incomingConnections.Remove(ic);
                    Log.LogMessage("Client disconnected while handshaking.");
                }
            }

            //Active connections
            foreach (var ac in _activeConnections.ToArray())
            {
                if (!ac.IsActive)
                {
                    ac.Close();
                    _activeConnections.Remove(ac);
                    Log.LogMessage("Client disconnected.");
                }
            }
        }

        private void ListenForUdp()
        {
            lock (_udpListener.Lock)
            {
                try
                {
                    while (_udpListener.Value.Available > 0)
                    {
                        IPEndPoint remoteUdpEndPoint = null;
                        byte[] data = _udpListener.Value.Receive(ref remoteUdpEndPoint);

                        //If from active connection
                        var activeCon = _activeConnections.FirstOrDefault((hc) => hc.RemoteUdpEndPoint.Equals(remoteUdpEndPoint));
                        if (activeCon != null)
                        {
                            NetPacket p = null;
                            try
                            {
                                p = DecryptPacket(data);
                            }
                            catch (FormatException)
                            {
                                Log.LogWarning("Received invalid packet from active connection.");
                            }

                            if (p != null)
                            {
                                activeCon.AddUdpPacket(p);
                            }
                        }
                        else //Else, token packet
                        {
                            if (data.Length == sizeof(UInt32))
                            {
                                UInt32 token = BitConverter.ToUInt32(data, 0);

                                var incomingCon = _incomingConnections.FirstOrDefault((ic) => ic.Token.Equals(token));
                                if (incomingCon != null)
                                {
                                    _udpListener.Value.Send(new byte[] { 1 }, 1, remoteUdpEndPoint);
                                    _incomingConnections.Remove(incomingCon);
                                    QueueNewConnection(incomingCon, remoteUdpEndPoint);
                                }
                                else
                                {
                                    Log.LogWarning("Received invalid token.");
                                }
                            }
                            else
                            {
                                Log.LogWarning("Received invalid packet from unknown source: " + data.Length + " != " + sizeof(UInt32));
                            }
                        }
                    }
                }
                catch (SocketException)
                { }
            }
        }

        //////////////////////

        private void QueueNewConnection(IncomingConnection incomingCon, IPEndPoint remoteUdpEndPoint)
        {
            lock (_newConnections.Lock)
            {
                HostingConnection newHostingConnection = new HostingConnection(incomingCon.TcpConnection, _udpListener, remoteUdpEndPoint);
                newHostingConnection.Log.LinkedLogger = this._log;
                _activeConnections.Add(newHostingConnection);
                _newConnections.Value.Enqueue(newHostingConnection);
            }

            Log.LogMessage("New connection: "
                + ((IPEndPoint)incomingCon.TcpConnection.RemoteEndPoint).Address.ToString()
                + "/"
                + ((IPEndPoint)incomingCon.TcpConnection.RemoteEndPoint).Port.ToString());
        }

        private UInt32 GenerateRandomToken()
        {
            UInt32 token;
            do
            {
                token = (UInt32)_tokenRandomizer.Next(int.MinValue, int.MaxValue);
            } while (_incomingConnections.Any((ic) => ic.Token.Equals(token)));

            return token;
        }

        private NetPacket DecryptPacket(byte[] data)
        {
            byte packetId = data[0];
            int packetLength = NetPacket.PacketPrefixLength + BitConverter.ToInt32(data, 1);

            if (data.Length + 1 < packetLength)
                throw new FormatException("Packet does not match prefix info.");

            NetPacket packet = _packetDecryptor(packetId, data.Skip(1).Take(packetLength - 1).ToArray());
            if (packet == null)
                throw new FormatException("Error while decrypting packet from receivebuffer.");

            return packet;
        }

        //////////////////////

        /// <summary>
        /// True when a connection is pending to be taken by GetConnection.
        /// </summary>
        public bool ConnectionAvailable
        {
            get
            {
                return _newConnections.Value.Count > 0;
            }
        }

        /// <summary>
        /// Returns a pending connection or null if there is none.
        /// </summary>
        public INetConnection GetConnection()
        {
            lock (_newConnections.Lock)
            {
                if (_newConnections.Value.Count > 0)
                    return _newConnections.Value.Dequeue();
                else
                    return null;
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
