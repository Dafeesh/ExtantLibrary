using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Extant;
using Extant.Unity;

namespace ExtantTests.Unity
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void Vector3_AsEquals()
        {
            Vector3 vec = new Vector3(1, 2, 3);
            Vector3 vecZero = new Vector3(0, 0, 0);
            Vector3 vecEquiv = vecZero.AsXEquals(1).AsYEquals(2).AsZEquals(3);

            bool vecEqualsEquiv = vec.Equals(vecEquiv);

            Assert.IsTrue(vecEqualsEquiv, vec.ToString() + " == " + vecEquiv.ToString());
        }

        [TestMethod]
        public void Float2_Conversions()
        {
            float2 oneonef2 = new float2(1f, 1f);
            Vector2 oneonev2 = new Vector2(1f, 1f);

            bool f2vEquals = oneonef2.ToVector2().Equals(oneonev2);
            Assert.IsTrue(f2vEquals, "(Vector2)float2(1,1) == Vector2(1,1)");

            bool v2fEquals = oneonev2.ToFloat2().Equals(oneonef2);
            Assert.IsTrue(v2fEquals, "(float2)Vector2(1,1) == float2(1,1)");
        }

        [TestMethod]
        public void Float2_Conversions_Array()
        {
            float[][] rawValues =
            {
                new float[] { 0, 0},
                new float[] { .5f, .5f },
                new float[] { -.5f, -.5f }
            };

            float2[] f2arrEmpty = new float2[0];
            Vector2[] v2arrEmpty = new Vector2[0];
            float2[] f2arr =
            {
                new float2(rawValues[0][0], rawValues[0][1]),
                new float2(rawValues[1][0], rawValues[1][1]),
                new float2(rawValues[2][0], rawValues[2][1])
            };
            Vector2[] v2arr =
            {
                new Vector2(rawValues[0][0], rawValues[0][1]),
                new Vector2(rawValues[1][0], rawValues[1][1]),
                new Vector2(rawValues[2][0], rawValues[2][1])
            };

            bool f2vArrEquals = f2arr.ToVector2Array().SequenceEqual(v2arr);
            Assert.IsTrue(f2vArrEquals, "(Vector2[])float2[] == Vector2[]");

            bool v2fArrEquals = v2arr.ToFloat2Array().SequenceEqual(f2arr);
            Assert.IsTrue(v2fArrEquals, "(float2[])Vector2[] == float2[]");

            bool f2vEmptyArrEquals = f2arr.ToVector2Array().SequenceEqual(v2arr);
            Assert.IsTrue(f2vEmptyArrEquals, "(Vector2[])float2[0] == Vector2[0]");

            bool v2fEmptyArrEquals = v2arr.ToFloat2Array().SequenceEqual(f2arr);
            Assert.IsTrue(v2fEmptyArrEquals, "(float2[])Vector2[0] == float2[0]");
        }
    }
}
