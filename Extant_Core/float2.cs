using System;
using System.Collections.Generic;

using ProtoBuf;

namespace Extant
{
    [ProtoContract]
    public struct float2
    {
        public static readonly float2 Zero = new float2(0, 0);
        public static readonly float2 Epsilon = new float2(float.Epsilon, float.Epsilon);

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

        public static float2 operator *(float2 a, float m)
        {
            return new float2(a.X * m, a.Y * m);
        }

        public static float2 operator /(float2 a, float m)
        {
            return new float2(a.X / m, a.Y / m);
        }

        public static float2 operator +(float2 a, float2 b)
        {
            return new float2(a.X + b.X, a.Y + b.Y);
        }

        public static float2 operator -(float2 a, float2 b)
        {
            return new float2(a.X - b.X, a.Y - b.Y);
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
        }

        public float2 Normal()
        {
            float mag = this.Magnitude();
            return new float2(this.X / mag, this.Y / mag);
        }
        
        public float2 Inverse()
        {
            return new float2(-this.X, -this.Y);
        }

        public float DistanceTo(float2 otherPoint)
        {
            return (float)Math.Sqrt(Math.Pow(this.X - otherPoint.X, 2) + Math.Pow(this.Y - otherPoint.Y, 2));
        }

        public float2 StepTowards(float2 other, float dist)
        {
            if (dist >= this.DistanceTo(other))
                return other;
            else
                return this + ((other - this).Normal() * dist);
        }

        public bool WithinArea(float minX, float minY, float maxX, float maxY)
        {
            return !(this.X < minX || this.X > maxX ||
                     this.Y < minY || this.Y > maxY);
        }

        public float2 Scale(float m)
        {
            return new float2(this.X * m, this.Y * m);
        }

        public float2 Scale(float2 m2)
        {
            return new float2(this.X * m2.X, this.Y * m2.Y);
        }

        public float2 Rotate(float degrees)
        {
            float sin = (float)Math.Sin(degrees * (Math.PI / 180f));
            float cos = (float)Math.Cos(degrees * (Math.PI / 180f));
            return new float2((cos * this.X) - (sin * this.Y), (sin * this.X) + (cos * this.Y));
        }

        public float ToDegrees()
        {
            return (float)(Math.Atan2(this.Y, this.X) * 180.0f / Math.PI);
        }

        public static float2 FromDegrees(float degrees)
        {
            float angle = (float)Math.PI * degrees / 180.0f;
            return new float2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}
