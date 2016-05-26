using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Extant;
using Extant.Util;
using Extant.Extensions;

namespace ExtantTests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void IEnumerationT_ToAggregateString()
        {
            string str = "Hello123";
            string separator = "\n-";

            string[] arr = new string[]
            {
                str, str, str
            };

            string expectedNoTrail = str + separator + str + separator + str;
            string expectedWithTrail = str + separator + str + separator + str + separator;

            string noTrail = arr.ToAggregateString(separator, false);
            string withTrail = arr.ToAggregateString(separator, true);

            Assert.IsTrue(noTrail.Equals(expectedNoTrail), "Without trail failed.");
            Assert.IsTrue(withTrail.Equals(expectedWithTrail), "With trail failed.");
        }
    }
}
