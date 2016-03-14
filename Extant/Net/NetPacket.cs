using System;
using System.Collections.Generic;
using System.Linq;

using ProtoBuf;
using System.IO;

namespace Extant.Net
{
    /// <summary>
    /// This is the base class for what is send between client and server of a ClientConnection, TCP/UdpConnection.
    /// Extend this class and define your class using protobuf's headers. Be sure to define the ID of the packet
    ///  in the default constructor.
    /// </summary>
    public abstract class NetPacket
    {
        public delegate NetPacket PacketDecryptor(byte packetId, byte[] buffer);
        public const int PacketPrefixLength = sizeof(byte) + sizeof(Int32);

        private byte _id;

        protected NetPacket(byte id)
        {
            this._id = id;
        }

        public byte[] GetBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(this.PacketByteID);
                Serializer.NonGeneric.SerializeWithLengthPrefix(ms, this, PrefixStyle.Fixed32, 0);

                return ms.ToArray();
            }
        }

        public static T GetFromBuffer<T>(byte[] buffer, int id) where T : NetPacket
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                return Serializer.DeserializeWithLengthPrefix<T>(ms, PrefixStyle.Fixed32, id);
            }
        }

        public byte PacketByteID
        {
            get
            {
                return _id;
            }
        }
    }
}
