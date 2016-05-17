using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Extant;
using Extant.Unity;

namespace ExtantTests
{
    [TestClass]
    public class Float2Tests
    {
        [TestMethod]
        public void Float2_StepTowards()
        {
            float2 one_zero = new float2(1f, 0f);
            float2 zero_one = new float2(0f, 1f);
            float2 one_one = new float2(1f, 1f);
            float2 zero2 = new float2(0f, 0f);

            float step10to00by1 = one_zero.StepTowards(zero2, 1f).Magnitude();
            Assert.IsTrue(step10to00by1 < float.Epsilon, "(1,0) step 1 == " + step10to00by1);

            float step01to00by1 = zero_one.StepTowards(zero2, 1f).Magnitude();
            Assert.IsTrue(step01to00by1 < float.Epsilon, "(0,1) step 1 == " + step01to00by1);

            float step11to00by2 = one_one.StepTowards(zero2, 2f).Magnitude();
            Assert.IsTrue(step11to00by2 < float.Epsilon, "(1,1) step 2 == " + step11to00by2);
        }
    }
}
