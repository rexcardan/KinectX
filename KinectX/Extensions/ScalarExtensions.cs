using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Extensions
{
    public static class ScalarExtensions
    {
        public static double DistanceTo(this Scalar s, Scalar nextS)
        {
            var distanceSq = Math.Pow(nextS.Val0 - s.Val0, 2)
                + Math.Pow(nextS.Val1 - s.Val1, 2)
                + Math.Pow(nextS.Val2 - s.Val2, 2);
            return Math.Sqrt(distanceSq);
        }
    }
}
