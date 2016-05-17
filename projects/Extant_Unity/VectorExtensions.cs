using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Extant;

namespace Extant.Unity
{
    public static class VectorExtensions
    {
        public static float2 ToFloat2(this Vector2 vec2)
        {
            return new float2(vec2.x, vec2.y);
        }

        public static float2 ToFloat2(this Vector3 vec3)
        {
            return new float2(vec3.x, vec3.y);
        }

        public static float2[] ToFloat2Array(this ICollection<Vector2> vec2Col)
        {
            float2[] arr = new float2[vec2Col.Count];
            int i = 0;
            foreach (var v2 in vec2Col)
            {
                arr[i++] = v2.ToFloat2();
            }
            return arr;
        }

        public static Vector3[] ToVector3Array(this ICollection<Vector2> vec2Col)
        {
            Vector3[] arr = new Vector3[vec2Col.Count];
            int i = 0;
            foreach (var v2 in vec2Col)
            {
                arr[i++] = v2;
            }
            return arr;
        }
        
        /////////////////////////////////

        public static float DistanceTo(this Vector2 a, Vector2 b)
        {
            if (a.Equals(b))
                return 0f;

            return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
        }

        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        public static Vector3 ToXZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 AsXEquals(this Vector3 v, float f)
        {
            return new Vector3(f, v.y, v.z);
        }

        public static Vector3 AsYEquals(this Vector3 v, float f)
        {
            return new Vector3(v.x, f, v.z);
        }

        public static Vector3 AsZEquals(this Vector3 v, float f)
        {
            return new Vector3(v.x, v.y, f);
        }
    }
}