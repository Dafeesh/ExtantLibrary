using System;
using System.Xml.Serialization;

using ProtoBuf;

namespace Extant
{
    [XmlRoot("Int2"), ProtoContract]
    public struct int2
    {
        [XmlElement("X"), ProtoMember(1)]
        public int x;
        [XmlElement("Y"), ProtoMember(2)]
        public int y;

        public int2(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public override bool Equals(object other)
        {
            return
                other is int2 &&
                this == (int2)other;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(int2 a, int2 b)
        {
            return
                a.x.Equals(b.x) &&
                a.y.Equals(b.y);
        }

        public static bool operator !=(int2 a, int2 b)
        {
            return !(a == b);
        }

        public static int2 operator +(int2 a, int2 b)
        {
            return new int2(a.x + b.x, a.y + b.y);
        }

        public static int2 operator -(int2 a, int2 b)
        {
            return new int2(a.x - b.x, a.y - b.y);
        }

        public static explicit operator float2(int2 i)
        {
            return new float2(i.x, i.y);
        }

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(this.x * this.x + this.y + this.y);
            }
        }
    }
}
