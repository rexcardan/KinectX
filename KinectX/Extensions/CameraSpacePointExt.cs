using Microsoft.Kinect;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.Voxels.Extensions
{
    public static class CameraSpacePointExt
    {
        public static List<Point3f> ToPointCloud(this CameraSpacePoint[] cspts, MatOfFloat pose)
        {
            List<Point3f> cloud = new List<Point3f>();

            foreach (var cspt in cspts)
            {
                if (!float.IsNaN(cspt.X) && !float.IsNegativeInfinity(cspt.X)
                    && !float.IsPositiveInfinity(cspt.X))
                {
                    var pt = cspt.AsPoint3f();
                    var ptTx = pose.TransformPoint3f(pt);
                    cloud.Add(ptTx);
                }
            }        
            return cloud;
        }

        public static Point3f AsPoint3f(this CameraSpacePoint cspt)
        {
            return new Point3f(cspt.X, cspt.Y, cspt.Z);
        }

        public static Vec4f AsVec4f(this CameraSpacePoint cspt)
        {
            return new Vec4f(cspt.X, cspt.Y, cspt.Z, 1);
        }
    }
}
