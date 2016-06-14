using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ProtoBuf;
using Extant.Net.Contract;
using Extant.Extensions;

namespace Extant.Net
{
    public abstract class NetPacket
    {
        private int _id;

        protected NetPacket(int id)
        {
            this._id = id;
        }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    Serializer.NonGeneric.SerializeWithLengthPrefix(ms, this, PrefixStyle.Base128, _id);
                }
                catch (Exception e)
                {
                    throw new FormatException("Failed to serialize packet.", e);
                }
                return ms.ToArray();
            }
        }

        public int PacketID
        {
            get
            {
                return _id;
            }
        }

        //------------------------//

        public static bool TryDeserialize(MemoryStream buffer, Type contractGroup, out NetPacket packet)
        {
            object deserializedObject;
            try
            {
                buffer.MoveToStart();
                if (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(
                    buffer, PrefixStyle.Base128,
                    (id) => NetPacketResolver.Resolve(contractGroup, id),
                    out deserializedObject))
                {
                    packet = deserializedObject as NetPacket;
                    buffer.Shift(buffer.Position);
                    return true;
                }
                else
                {
                    packet = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                throw new FormatException("Error while trying to deserialize.", e);
            }
            finally
            {
                buffer.MoveToEnd();
            }
        }

        public static bool InitializeContractGroup(Type packetContainerClass)
        {
            string errorDump;
            if (NetPacketResolver.TryRegisterContractsInContainer(packetContainerClass, out errorDump))
                return true;
            else
            {
                Console.WriteLine("Failed to initialize contract group \"" + packetContainerClass.ToString() + "\"...\n" + errorDump);
                return false;
            }
        }
    }
}
