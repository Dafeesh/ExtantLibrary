using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;

namespace Extant
{
    [XmlRoot("Float2"), ProtoContract]
    public struct float2
    {
        public static readonly float2 Zero = new float2(0, 0);
        public static readonly float2 Epsilon = new float2(float.Epsilon, float.Epsilon);
        public static readonly float2 MinValue = new float2(float.MinValue, float.MinValue);
        public static readonly float2 MaxValue = new float2(float.MaxValue, float.MaxValue);

        [XmlElement("X"), ProtoMember(1)]
        public float x;
        [XmlElement("Y"), ProtoMember(2)]
        public float y;

        public float2(float x = 0, float y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
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
                a.x.Equals(b.x) &&
                a.y.Equals(b.y);
        }

        public static bool operator !=(float2 a, float2 b)
        {
            return !(a == b);
        }

        public static float2 operator *(float2 a, float m)
        {
            return new float2(a.x * m, a.y * m);
        }

        public static float2 operator /(float2 a, float m)
        {
            return new float2(a.x / m, a.y / m);
        }

        public static float2 operator +(float2 a, float2 b)
        {
            return new float2(a.x + b.x, a.y + b.y);
        }

        public static float2 operator -(float2 a, float2 b)
        {
            return new float2(a.x - b.x, a.y - b.y);
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(this.x * this.x + this.y * this.y);
        }

        public float2 Normalized()
        {
            float mag = this.Magnitude();
            return new float2(this.x / mag, this.y / mag);
        }

        public float DistanceTo(float2 otherPoint)
        {
            return (float)Math.Sqrt(Math.Pow(this.x - otherPoint.x, 2) + Math.Pow(this.y - otherPoint.y, 2));
        }

        public float2 StepTowards(float2 other, float dist, bool overStep = false)
        {
            if (!overStep && dist >= this.DistanceTo(other))
                return other;
            else
                return this + ((other - this).Normalized() * dist);
        }

        public bool WithinArea(float minX, float minY, float maxX, float maxY)
        {
            return !(this.x < minX || this.x > maxX ||
                     this.y < minY || this.y > maxY);
        }

        public float2 Scale(float m)
        {
            return new float2(this.x * m, this.y * m);
        }

        public float2 Scale(float2 m2)
        {
            return new float2(this.x * m2.x, this.y * m2.y);
        }

        public float2 Rotate(float degrees)
        {
            float sin = (float)Math.Sin(degrees * (Math.PI / 180f));
            float cos = (float)Math.Cos(degrees * (Math.PI / 180f));
            return new float2((cos * this.x) - (sin * this.y), (sin * this.x) + (cos * this.y));
        }

        public float ToDegrees()
        {
            return (float)(Math.Atan2(this.y, this.x) * 180.0f / Math.PI);
        }

        public static float2 FromDegrees(float degrees)
        {
            float angle = (float)Math.PI * degrees / 180.0f;
            return new float2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}
