using KinectX.Data;
using KinectX.Registration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Extensions
{
    public static class CvColorExtensions
    {
        public static List<Marker> FindAruco(this CvColor cvColor)
        {
            return Vision.FindAruco(cvColor);
        }

        public static Mat DrawAruco(this CvColor cvColor)
        {
           return Vision.DrawAruco(cvColor);
        }
    }
}
