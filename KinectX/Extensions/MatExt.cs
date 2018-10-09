using Microsoft.Kinect;
using Microsoft.Kinect.Fusion;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Extensions
{
    public static class MatExt
    {
        public static Point3f TransformPoint3f(this MatOfFloat pose, Point3f point)
        {
            if(pose.Cols!=4 || pose.Rows != 4)
            {
                throw new ArgumentException("Pose must be a 4 x 4 matrix");
            }

            var m1 = new float[4, 4];
            pose.GetArray(0, 0, m1);

            var v1 = new float[4] { point.X, point.Y, point.Z, 1 };

            var homogenous =  new Vec4f(
               (m1[0, 0] * v1[0] + m1[0, 1] * v1[1] + m1[0, 2] * v1[2] + m1[0, 3] * v1[3]),
               (m1[1, 0] * v1[0] + m1[1, 1] * v1[1] + m1[1, 2] * v1[2] + m1[1, 3] * v1[3]),
               (m1[2, 0] * v1[0] + m1[2, 1] * v1[1] + m1[2, 2] * v1[2] + m1[2, 3] * v1[3]),
               (m1[3, 0] * v1[0] + m1[3, 1] * v1[1] + m1[3, 2] * v1[2] + m1[3, 3] * v1[3])
               );

            return new Point3f(homogenous.Item0, homogenous.Item1, homogenous.Item2);
        }

        public static double[,] To2DArray(this MatOfFloat pose)
        {
            var m1 = new float[4, 4];
            var m2 = new double[4, 4];
            pose.GetArray(0, 0, m1);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m2[i, j] = m1[i, j];
                }
            }
            return m2;
        }

        public static Matrix4 ToKinectMatrix4(this MatOfFloat pose)
        {
            var m1 = new float[4, 4];
            pose.GetArray(0, 0, m1);
            var matrix = new Matrix4();
            matrix.M11 = m1[0, 0];
            matrix.M12 = m1[0, 1];
            matrix.M13 = m1[0, 2];
            matrix.M14 = m1[0, 3];

            matrix.M21 = m1[1, 0];
            matrix.M22 = m1[1, 1];
            matrix.M23 = m1[1, 2];
            matrix.M24 = m1[1, 3];

            matrix.M31 = m1[2, 0];
            matrix.M32 = m1[2, 1];
            matrix.M33 = m1[2, 2];
            matrix.M34 = m1[2, 3];

            matrix.M41 = m1[3, 0];
            matrix.M42 = m1[3, 1];
            matrix.M43 = m1[3, 2];
            matrix.M44 = m1[3, 3];

            return matrix;
        }

        public static void Show(this Mat mat)
        {
            Cv2.ImShow("Mat", mat);
            Cv2.WaitKey();
        }

        public static void ShowNoWait(this Mat mat)
        {
            Cv2.ImShow("Mat", mat);
            Cv2.WaitKey(10);
        }

        /// <summary>
        /// Mask for values which are real locations
        /// </summary>
        public static Mat GetRealMask(this Mat mat)
        {
            return mat.InRange(new MatOfFloat(1, 1, float.MinValue), new MatOfFloat(1, 1, float.MaxValue));
        }

        /// <summary>
        /// Mask for values which are inf, inf, inf or NaN, Nan, Nan
        /// </summary>
        /// <returns></returns>
        public static Mat GetIndeterminteMask(this Mat mat)
        {
            var mask = new Mat();
            Cv2.BitwiseNot(mat.GetRealMask(), mask);
            return mask;
        }

        public static List<Point3f> AsPoint3fs(this Mat mat)
        {
            if (mat.Channels() == 4)
            {
                var vals = new Vec4f[mat.Total()];
                mat.GetArray(0, 0, vals);
                return vals.Select(v => new Point3f(v.Item0, v.Item1,  v.Item2)).ToList();
            }
            else
            {
                var vals = new Point3f[mat.Total()];
                mat.GetArray(0, 0, vals);
                return vals.ToList();
            }
        }

        public static CameraSpacePoint[] ToCamSpacePoints(this Mat mat)
        {
            if (mat.Channels() == 4)
            {
                var vals = new Vec4f[mat.Total()];
                mat.GetArray(0, 0, vals);
                return vals.Select(v => new CameraSpacePoint() { X = v.Item0, Y = v.Item1, Z = v.Item2 }).ToArray();
            }
            else
            {
                var vals = new Point3f[mat.Total()];
                mat.GetArray(0, 0, vals);
                return vals.Select(v => new CameraSpacePoint() { X = v.X, Y = v.Y, Z = v.Z }).ToArray();
            }
            
        }
    }
}
