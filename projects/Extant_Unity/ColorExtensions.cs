using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Unity
{
    public static class ColorExtensions
    {
        public static Color WithAlphaAs(this Color c, float a)
        {
            c.a = a;
            return c;
        }
    }
}
