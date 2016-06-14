using System;
using System.Collections.Generic;
using System.Linq;

using Extant.Threading;
using Extant.Util;

namespace Extant.Net
{
    public class PacketQueue
    {
        private Type _packetGroup;
        private LockValuePair<Queue<NetPacket>> _packets = new LockValuePair<Queue<NetPacket>>(new Queue<NetPacket>());
        private SimpleTimer _lastPacketTimer = SimpleTimer.StartNew();

        public PacketQueue(Type packetGroup)
        {
            if (packetGroup == null)
                throw new ArgumentNullException();

            this._packetGroup = packetGroup;
        }

        public void Enqueue(NetPacket packet)
        {
            if (packet == null)
                throw new ArgumentNullException();

            lock (_packets.Lock)
            {
                _packets.Value.Enqueue(packet);
            }

            _lastPacketTimer.Restart();
        }

        public NetPacket PollDequeue()
        {
            lock (_packets.Lock)
            {
                if (!HasNext)
                    return null;

                return _packets.Value.Dequeue();
            }
        }

        public Type PacketGroup
        {
            get
            {
                return _packetGroup;
            }
        }

        public TimeSpan TimeSinceLastEnqueue
        {
            get
            {
                return _lastPacketTimer.Elapsed;
            }
        }

        public bool HasNext
        {
            get
            {
                return _packets.Value.Count > 0;
            }
        }
    }
}
