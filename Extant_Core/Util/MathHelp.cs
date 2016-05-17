using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Util
{
    public static class MathHelp
    {
        public static float2 ReflectionNormal(float2 inward, float2 normal)
        {
            return inward.Inverse().Rotate((normal.ToDegrees() - inward.Inverse().ToDegrees()) * 2f);
        }
    }
}
