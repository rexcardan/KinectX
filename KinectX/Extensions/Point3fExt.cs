using OpenCvSharp;
using System;

namespace KinectX.Extensions
{
    public static class Point3fExt
    {
        public static float Magnitude(this Point3f point)
        {
            return (float)Math.Sqrt(point.X * point.X + point.Y * point.Y + point.Z * point.Z);
        }

        public static Point3f Normalize(this Point3f point)
        {
            var mag = point.Magnitude();
            return new Point3f(point.X / mag, point.Y / mag, point.Z / mag);
        }
    }
}
