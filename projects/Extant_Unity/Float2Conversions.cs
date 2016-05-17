using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Extant.Unity
{
    public static class Float2Conversions
    {
        public static Vector2 ToVector2(this float2 f2)
        {
            return new Vector2(f2.x, f2.y);
        }

        public static bool Equals(this float2 f2, Vector2 v2)
        {
            return f2.x == v2.x && f2.y == v2.y;
        }

        public static Vector2[] ToVector2Array(this ICollection<float2> f2Col)
        {
            Vector2[] arr = new Vector2[f2Col.Count];
            int i = 0;
            foreach (var f2 in f2Col)
            {
                arr[i++] = f2.ToVector2();
            }
            return arr;
        }

        public static Vector3[] ToVector3Array(this ICollection<float2> f2Col)
        {
            Vector3[] arr = new Vector3[f2Col.Count];
            int i = 0;
            foreach (var f2 in f2Col)
            {
                arr[i++] = f2.ToVector2();
            }
            return arr;
        }
    }
}
