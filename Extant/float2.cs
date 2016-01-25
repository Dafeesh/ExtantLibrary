using System;
using System.Collections.Generic;

using ProtoBuf;

namespace Extant
{
    [ProtoContract]
    public struct float2
    {
        public static readonly float2 Zero = new float2(0, 0);

        [ProtoMember(1)]
        public float X;
        [ProtoMember(2)]
        public float Y;

        public float2(float x = 0, float y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return
                other is float2 &&
                this == (float2)other;
        }

        public static bool operator ==(float2 a, float2 b)
        {
            return 
                a.X.Equals(b.X) && 
                a.Y.Equals(b.Y);
        }

        public static bool operator !=(float2 a, float2 b)
        {
            return !(a == b);
        }

        public static float2 operator +(float2 a, float2 b)
        {
            return new float2(a.X + b.X, a.Y + b.Y);
        }

        public static float2 operator -(float2 a, float2 b)
        {
            return new float2(a.X - b.X, a.Y - b.Y);
        }

        public float DistanceTo(float2 otherPoint)
        {
            return (float)Math.Sqrt(Math.Pow(this.X - otherPoint.X, 2) + Math.Pow(this.Y - otherPoint.Y, 2));
        }

        public bool WithinArea(float minX, float minY, float maxX, float maxY)
        {
            return !(this.X < minX || this.X > maxX ||
                     this.Y < minY || this.Y > maxY);
        }

        //public float2 AddEpsilon(float2 awayFrom)
        //{
        //    const float epsilon = 0.0001f;
        //    float xDiff = this.X - awayFrom.X;
        //    float yDiff = this.Y - awayFrom.Y;

        //    float2 newFP = this;

        //    if (xDiff < 0)
        //        newFP.X -= epsilon;
        //    else if (xDiff > 0)
        //        newFP.X += epsilon;

        //    if (yDiff < 0)
        //        newFP.Y -= epsilon;
        //    else if (yDiff > 0)
        //        newFP.Y += epsilon;

        //    return newFP;
        //}
    }
}
