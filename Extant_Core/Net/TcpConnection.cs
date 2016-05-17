using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Extant.Logging;

namespace Extant.Net
{
    public class TcpConnection : IDebugLogging
    {
        private TcpClient _tcpClient;

        private IPEndPoint _remoteEndPoint;
        private readonly Stopwatch _lastReceiveTimer;

        private NetworkStream _networkStream;
        private object _networkStream_lock = new object();

        private List<byte> _receiveBuffer = new List<byte>();
        private Byte[] _tempReceiveBuffer = new Byte[1024];

        private NetPacket.PacketDecryptor _packetDecryptor;
        private Queue<NetPacket> _packets = new Queue<NetPacket>();
        private object _packets_lock = new object();

        private DebugLogger _log;
        private ByteRecord _byteLog_out = new ByteRecord();
        private ByteRecord _byteLog_in = new ByteRecord();
        private bool _isClosed = false;

        public TcpConnection(TcpClient tcpClient, NetPacket.PacketDecryptor packetDecryptor)
        {
            this._log = new DebugLogger("TcpConnection");

            this._remoteEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
            this._packetDecryptor = packetDecryptor;

            this._tcpClient = tcpClient;
            this._networkStream = tcpClient.GetStream();

            this._lastReceiveTimer = Stopwatch.StartNew();
            BeginReceive();
        }

        /// <summary>
        /// Closes an active connection.
        /// </summary>
        public void Close()
        {
            if (!IsClosed)
            {
                this._tcpClient.Close();
                this._isClosed = true;

                Log.LogMessage("Closed.");
            }
        }

        private void BeginReceive()
        {
            this._tcpClient.Client.BeginReceive(_tempReceiveBuffer, 0, _tempReceiveBuffer.Length, SocketFlags.None, Callback_Receive, this._tcpClient.Client);
        }

        private void Callback_Receive(IAsyncResult ar)
        {
            if (!IsClosed)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;

                    int numBytes = client.EndReceive(ar);
                    if (numBytes == 0)
                    {
                        Log.LogMessage("ReceiveCallback, lost connection.");
                        this.Close();
                    }
                    else
                    {
                        _lastReceiveTimer.Reset();
                        _lastReceiveTimer.Start();

                        _byteLog_in.Bytes += numBytes;

                        _receiveBuffer.AddRange(_tempReceiveBuffer.Take(numBytes));
                        this.InterpretReceiveBuffer();

                        Log.LogMessage("ReceiveCallback, received bytes- " + numBytes);
                        BeginReceive();
                    }
                }
                catch (ObjectDisposedException)
                {
                    Log.LogMessage("ReceiveCallback, disposed.");
                    this.Close();
                }
                catch (Exception e)
                {
                    Log.LogMessage("ReceiveCallback exception: " + e.ToString());
                    this.Close();
                }
            }
        }

        private void InterpretReceiveBuffer()
        {
            if (_receiveBuffer.Count > NetPacket.PacketPrefixLength)
            {
                byte[] buffer = _receiveBuffer.ToArray();
                int bytesRead = 0;

                while (buffer.Length > NetPacket.PacketPrefixLength)
                {
                    byte packetId = buffer[0];
                    int packetLength = NetPacket.PacketPrefixLength + BitConverter.ToInt32(buffer, 1);

                    if (buffer.Length < packetLength)
                        break;

                    NetPacket packet = _packetDecryptor(packetId, buffer.Skip(1).Take(packetLength - 1).ToArray());
                    if (packet == null)
                        throw new FormatException("Error while interpretting packet from receivebuffer.");

                    bytesRead += packetLength;

                    lock (_packets_lock)
                    {
                        _packets.Enqueue(packet);
                    }

                    buffer = buffer.Skip(packetLength).ToArray();
                }

                if (bytesRead > 0)
                {
                    _receiveBuffer.RemoveRange(0, bytesRead);
                }
            }
        }

        /// <summary>
        /// Returns the oldest packet in the queue.
        /// If no packets available then this returns null.
        /// </summary>
        /// <returns></returns>
        public NetPacket GetPacket()
        {
            lock (_packets_lock)
            {
                if (_packets.Count > 0)
                {
                    return _packets.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Send a packet to this client.
        /// </summary>
        public void SendPacket(NetPacket p)
        {
            if (_tcpClient.Connected)
            {
                try
                {
                    Byte[] d = p.GetBytes();
                    lock (_networkStream_lock)
                    {
                        _byteLog_out.Bytes += d.Length;
                        _networkStream.Write(d, 0, d.Length);
                        _networkStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    Log.LogMessage("Exception while while sending packet: " + e.Message);
                    this.Close();
                }
            }
        }

        /// <summary>
        /// True if a packet is ready to received via .GetPacket().
        /// </summary>
        public bool PacketAvailable
        {
            get
            {
                lock (_packets_lock)
                {
                    return _packets.Count > 0;
                }
            }
        }

        /// <summary>
        /// True if the connection is currently still being used.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (!this.IsClosed || this.PacketAvailable);
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return _remoteEndPoint;
            }
        }

        public IDebugLogger Log
        {
            get
            {
                return _log;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _isClosed;
            }
        }
    }
}
