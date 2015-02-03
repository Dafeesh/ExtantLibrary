#define DEBUG_PACKETS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Extant.Networking
{
    /// <summary>
    /// Packets are containers of information to be sent over the network.
    /// Names are based on this convention: (Topic)_(Action)_(Target)
    /// Targets are as follows:
    ///     c = Player Client
    ///     g = GameServer
    /// </summary>
    public abstract class Packet
    {
        public static readonly Int32 STRBYTELENGTH_12 = sizeof(char) * 12;
        public static readonly Int32 STRBYTELENGTH_25 = sizeof(char) * 25;
        public static readonly Byte EMPTY_BYTE = (Byte)0;
        public static readonly Byte END_PACKET = (Byte)23; //End of trans. block
        public static readonly Char EMPTY_CHAR = (Char)0;
        public static readonly Byte BYTE_TRUE = (Byte)1;
        public static readonly Byte BYTE_FALSE = (Byte)0;

        protected Int32 type;
        private ProtocolType protocol;

        public abstract Byte[] CreateSendBuffer();

        protected Packet(Int32 t, ProtocolType p)
        {
            type = t;
            protocol = p;
        }

        public Int32 Type
        {
            get
            {
                return type;
            }
        }

        public ProtocolType Protocol
        {
            get
            {
                return protocol;
            }
        }

        /// <summary>
        /// Returns an Int32 that is read and removed from beginning of List of Bytes.
        /// </summary>
        /// <param name="buff">The array for data to be taken from.</param>
        public static Int32 TakeInt32(ref List<Byte> buff)
        {
            Int32 read = BitConverter.ToInt32(buff.ToArray(), 0);
            buff = buff.Skip(sizeof(Int32)).ToList();
            return read;
        }

        /// <summary>
        /// Returns a Double that is read and removed from beginning of List of Bytes.
        /// </summary>
        /// <param name="buff">The array for data to be taken from.</param>
        public static Double TakeDouble(ref List<Byte> buff)
        {
            Double read = BitConverter.ToDouble(buff.ToArray(), 0);
            buff = buff.Skip(sizeof(Double)).ToList();
            return read;
        }

        /// <summary>
        /// Returns a Byte that is read and removed from beginning of List.
        /// </summary>
        /// <param name="buff">The array for data to be taken from.</param>
        public static Byte TakeByte(ref List<Byte> buff)
        {
            Byte read = buff[0];
            buff = buff.Skip(sizeof(Byte)).ToList();
            return read;
        }

        /// <summary>
        /// Returns a String of Chars that is read and removed from beginning of List of Bytes.
        /// </summary>
        /// <param name="buff">The array for data to be taken from.</param>
        public static Char[] TakeUnicodeChars(ref List<Byte> buff, int count)
        {
            Char[] arr = Encoding.Unicode.GetChars(buff.ToArray(), 0, count);
            int returnAmount = 0;
            buff = buff.Skip(count).ToList();
            foreach (Char c in arr)
            {
                if (c == EMPTY_CHAR)
                    break;
                returnAmount++;
            }
            return arr.Take(returnAmount).ToArray();
        }

        /// <summary>
        /// Returns the Bytes that make up an Int32.
        /// </summary>
        public static Byte[] GetBytes_Int32(Int32 i)
        {
            return BitConverter.GetBytes(i);
        }

        /// <summary>
        /// Returns the Bytes that make up a Double.
        /// </summary>
        public static Byte[] GetBytes_Double(Double d)
        {
            return BitConverter.GetBytes(d);
        }

        /// <summary>
        /// Returns the Bytes that make up an array of Chars.
        /// </summary>
        /// <param name="str">String to read from.</param>
        /// <param name="size">Number of characters to read.</param>
        /// <returns></returns>
        public static Byte[] GetBytes_String_Unicode(String str, int size)
        {
            char[] stra = str.ToArray();

            List<Byte> arr = new List<Byte>();
            arr.AddRange(Encoding.Unicode.GetBytes(stra, 0, stra.Length).ToList());
            while (arr.Count < size)
            {
                arr.Add(EMPTY_BYTE);
            }
            return arr.ToArray();
        }

        /// <summary>
        /// Thrown if a packet is not a valid set of data for the corresponding header..
        /// </summary>
        public class InvalidPacketRead : Exception
        {
            public InvalidPacketRead()
                : base("Packet was found to be invalid upon reading.")
            { }
        }
    }
}