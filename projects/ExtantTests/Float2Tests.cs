using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using UnityEngine;
using Extant;
using Extant.Unity;
using Extant.Util;

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

        [TestMethod]
        public void Float2_XmlSerialization()
        {
            float2 single = new float2(2.2f, -9.9f);
            float2[] arr =
            {
                new float2(1f, 1f),
                new float2(-1f, -1f),
                new float2(0f, 0f)
            };

            string singleXmlString = XmlBuilder.BuildString(single);
            float2 parsedXmlObject = XmlBuilder.ParseString<float2>(singleXmlString);
            Assert.IsTrue(single.Equals(parsedXmlObject), "Reserialized single does not equal original.");

            string arrXmlString = XmlBuilder.BuildString(arr);
            float2[] parsedXmlArr = XmlBuilder.ParseString<float2[]>(arrXmlString);
            Assert.IsTrue(Enumerable.SequenceEqual(arr, parsedXmlArr), "Reserialized array does not equal original.");
        }
    }
}
