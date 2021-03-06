﻿using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Extensions
{
    public static class PointExtensions
    {
        public static Point3f[] BoundBy(this IEnumerable<Point3f> cpts, double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            return cpts.Where(c =>
            {
                return c.X >= minX && c.X <= maxX && c.Y >= minY && c.Y <= maxY && c.Z >= minZ && c.Z <= maxZ;
            }).ToArray();
        }
    }
}
