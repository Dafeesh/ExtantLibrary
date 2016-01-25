using System;

using ProtoBuf;

namespace Extant
{
    [ProtoContract]
    public struct int2
    {
        [ProtoMember(1)]
        public int X;
        [ProtoMember(2)]
        public int Y;

        public int2(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        public override bool Equals(object other)
        {
            return
                other is int2 &&
                this == (int2)other;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(int2 a, int2 b)
        {
            return
                a.X.Equals(b.X) &&
                a.Y.Equals(b.Y);
        }

        public static bool operator !=(int2 a, int2 b)
        {
            return !(a == b);
        }

        public static float2 operator +(int2 a, int2 b)
        {
            return new float2(a.X + b.X, a.Y + b.Y);
        }

        public static float2 operator -(int2 a, int2 b)
        {
            return new float2(a.X - b.X, a.Y - b.Y);
        }
    }
}
