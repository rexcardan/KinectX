using Microsoft.VisualStudio.TestTools.UnitTesting;
using KinectX.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Extensions.Tests
{
    [TestClass()]
    public class Array2DExtensionsTests
    {
        [TestMethod()]
        public void ToMatTest()
        {
            var testArray = new double[4, 4]
            {
                { 1,2,3,4 },
                {5,6,7,8 },
                {9,10,11,12 },
                {13,14,15,16 }
            };

            var mat = testArray.ToMat();
            double[] vals = new double[16];
            mat.GetArray(0, 0,vals);
            for (int i = 0; i < 16; i++)
            {
                var test = i % 4;
                Assert.AreEqual(testArray[i / 4, i % 4], vals[i]);
            }
            mat.Dispose();
        }
    }
}