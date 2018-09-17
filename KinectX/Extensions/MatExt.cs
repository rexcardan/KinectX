using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
