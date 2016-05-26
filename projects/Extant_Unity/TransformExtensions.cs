using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Extant.Unity
{
    public static class TransformExtensions
    {
        public static void ResetToZero(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        public static void ResetToLocalZero(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
