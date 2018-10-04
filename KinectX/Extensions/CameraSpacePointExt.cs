using Microsoft.Kinect;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Extensions
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

        /// <summary>
        /// Filters camera space points to ones where the coordinates are real. If the depth pixel was unknown, the corresponding camera space
        /// point will have infinitiy coordinates
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IEnumerable<CameraSpacePoint> Real(this IEnumerable<CameraSpacePoint> points)
        {
            if (points != null)
            {
                return points.Where(c => !double.IsInfinity(c.X) && !double.IsInfinity(c.Y) && !double.IsInfinity(c.Z) &&
                !double.IsNaN(c.X) && !double.IsNaN(c.Y) && !double.IsNaN(c.Z));
            }

            return points;
        }

        /// <summary>
        /// Filters camera space points to ones where the coordinates are real. If the depth pixel was unknown, the corresponding camera space
        /// point will have infinitiy coordinates
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static bool IsReal(this CameraSpacePoint c)
        {
            return !double.IsInfinity(c.X) && !double.IsInfinity(c.Y) && !double.IsInfinity(c.Z) &&
            !double.IsNaN(c.X) && !double.IsNaN(c.Y) && !double.IsNaN(c.Z);
        }
    }
}
