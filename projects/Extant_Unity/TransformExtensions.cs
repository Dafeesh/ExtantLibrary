using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Extant.Unity
{
    public static class TransformExtensions
    {
        public static void ResetToLocalZero(this Transform transform)
        {
            transform.localPosition = new Vector3();
            transform.localRotation = new Quaternion();
        }
    }
}
