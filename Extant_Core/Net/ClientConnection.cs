using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Extant.Util;
using Extant.Threading;
using Extant.Logging;

namespace Extant.Net
{
    public class ClientConnection : INetConnection, IDebugLogging
    {
        private TcpConnection _tcpConnection;
        private UdpConnection _udpConnection;

        private SimpleTimer _lastPacketTimer;

        private DebugLogger _log;

        private ClientConnection(TcpConnection tcpCon, UdpConnection udpCon)
        {
            this._tcpConnection = tcpCon;
            this._udpConnection = udpCon;

            this._log = new DebugLogger("CliCon");
            this._tcpConnection.Log.LinkedLogger = this._log;
            this._udpConnection.Log.LinkedLogger = this._log;
            this._log.IsPostingToConsole = true;

            this._lastPacketTimer = SimpleTimer.StartNew();
        }

        ////////////////////////////////////////////////

        public void Send(NetPacket packet, NetworkProtocol protocol)
        {
            switch (protocol)
            {
                case (NetworkProtocol.TCP):
                    _tcpConnection.SendPacket(packet);
                    break;

                case (NetworkProtocol.UDP):
                    _udpConnection.SendPacket(packet);
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
                _lastPacketTimer.Reset();
                _lastPacketTimer.Start();
                return _tcpConnection.GetPacket();
            }

            //UDP
            if (_udpConnection.PacketAvailable)
            {
                _lastPacketTimer.Reset();
                _lastPacketTimer.Start();
                return _udpConnection.GetPacket();
            }

            //None
            return null;
        }

        public void Close()
        {
            _tcpConnection.Close();
            _udpConnection.Close();
        }

        ////////////////////////////////////////////////

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
                return _tcpConnection.PacketAvailable || _udpConnection.PacketAvailable;
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

        ////////////////////////////////////////////////

        public class ConnectionProcess
        {
            public ClientConnection ClientConnection { get; private set; }
            public Step ConnectionStep { get; private set; }
            public bool IsDone { get; private set; }

            private Thread _thread;
            private IPEndPoint _tcpEP;
            private IPEndPoint _udpEP;
            private NetPacket.PacketDecryptor _decryptor;

            private ConnectionProcess(IPEndPoint tcpEP, IPEndPoint udpEP, NetPacket.PacketDecryptor decryptor)
            {
                this._thread = new Thread(new ThreadStart(_Run));
                this._thread.Name = "ConnectingProcess";
                this._tcpEP = tcpEP;
                this._udpEP = udpEP;
                this._decryptor = decryptor;

                this.ClientConnection = null;
                this.ConnectionStep = Step.Waiting;
                this.IsDone = false;
            }

            public static ConnectionProcess StartNew(IPEndPoint tcpEP, IPEndPoint udpEP, NetPacket.PacketDecryptor decryptor)
            {
                ConnectionProcess cp = new ConnectionProcess(tcpEP, udpEP, decryptor);
                cp.Start();
                return cp;
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Cancel()
            {
                _thread.Abort();
            }

            private void _Run()
            {
                TcpClient tcpClient = new TcpClient();
                try
                {
                    try
                    {
                        ConnectionStep = Step.Connecting;
                        tcpClient.Connect(_tcpEP);
                    }
                    catch (SocketException)
                    {
                        return;
                    }

                    if (tcpClient.Connected)
                    {
                        ConnectionStep = Step.Handshaking_TCP;

                        byte[] buffer = new byte[1028];
                        int received = tcpClient.Client.Receive(buffer);
                        if (received == sizeof(UInt32))
                        {
                            UInt32 token = BitConverter.ToUInt32(buffer, 0);

                            Thread.Sleep(100);

                            ConnectionStep = Step.Handshaking_UDP;

                            UdpClient udpClient = new UdpClient();
                            udpClient.Connect(_udpEP);
                            udpClient.Send(buffer, received);

                            IPEndPoint verifyUdpEndPoint = null;
                            byte[] verifyData = udpClient.Receive(ref verifyUdpEndPoint);
                            if (verifyData.Length == 1)
                            {
                                ConnectionStep = Step.Connected;

                                ClientConnection =
                                    new ClientConnection(
                                        new TcpConnection(tcpClient, _decryptor),
                                        new UdpConnection(udpClient, _decryptor));
                            }
                        }
                    }
                }
                finally
                {
                    if (ClientConnection == null)
                        tcpClient.Close();
                    IsDone = true;
                }
            }

            public enum Step
            {
                Waiting,
                Connecting,
                Handshaking_TCP,
                Handshaking_UDP,
                Connected
            }
        }
    }
}
