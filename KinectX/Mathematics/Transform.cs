using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace KinectX.Mathematics
{
    public class Transform
    {
        public static MatOfFloat TransformBetween(MatOfPoint3f origPoint3f, MatOfPoint3f destPoint3f)
        {
            int rowCountA = origPoint3f.Rows;
            int rowCountB = destPoint3f.Rows;

            if (rowCountA != rowCountB)
            {
                throw new Exception("Data must be paired. The number of rows should be equal in both input matrices!");
            }

            var orig = ConvertToFC1(origPoint3f);
            var dest = ConvertToFC1(destPoint3f);

            //OCV
            var origCentroid = GetCentroid(origPoint3f);
            var destCentroid = GetCentroid(destPoint3f);

            var aCentroidMatrix = new MatOfPoint3f();
            for (int rep = 0; rep < rowCountA; rep++) { aCentroidMatrix.Add(origCentroid); }
            var bCentroidMatrix = new MatOfPoint3f();
            for (int rep = 0; rep < rowCountB; rep++) { bCentroidMatrix.Add(destCentroid); }

            //OCV
            var step1 = Subtract(origPoint3f, aCentroidMatrix);
            var step2 = Subtract(destPoint3f, bCentroidMatrix);

            Mat reshapeStep1 = Reshape(step1);
            Mat reshapeStep2 = Reshape(step2);

            var h = (reshapeStep1.Transpose() * reshapeStep2).ToMat();
            Mat w = new Mat();
            Mat u = new Mat();
            Mat vt = new Mat();

            Cv2.SVDecomp(h, w, u, vt);

            Mat V = vt.Transpose();

            var r = (V * (u.Transpose())).ToMat();
            var first = r * (-1);
            var second = first.ToMat() * origCentroid;
            var t = (((r * (-1)) * (Reshape(origCentroid).Transpose())) + Reshape(destCentroid).Transpose()).ToMat();

            //Set translation component
            var tvals = new float[3];
            t.GetArray(0, 0, tvals);
            var mat = new MatOfFloat(4, 4);

            mat.Set(0, 3, tvals[0]);
            mat.Set(1, 3, tvals[1]);
            mat.Set(2, 3, tvals[2]);
            mat.Set(3, 3, 1.0f);


            ////Set rotation componen
            for (int row = 0; row < 3; row++)
            {
                var vals = new float[3];
                r.GetArray(row, 0, vals);
                mat.SetArray(row, 0, vals);
            }
            return mat;
        }

        private static Mat Reshape(MatOfPoint3f points)
        {
            Mat answer = new Mat(points.Rows, 3, MatType.CV_32FC1);
            for (int i = 0; i < points.Rows; i++)
            {
                var point = points.ElementAt(i);
                answer.Set<float>(i, 0, point.X);
                answer.Set<float>(i, 1, point.Y);
                answer.Set<float>(i, 2, point.Z);
            }
            return answer;
        }

        private static MatOfPoint3f Subtract(MatOfPoint3f orig, MatOfPoint3f aCentroidMatrix)
        {
            MatOfPoint3f answer = new MatOfPoint3f();
            for (int i = 0; i < orig.Rows; i++)
            {
                answer.Add(orig.ElementAt(i) - aCentroidMatrix.ElementAt(i));
            }
            return answer;
        }

        private static double[,] ConvertToDoubleArray(MatOfPoint3f aCentroidMatrix)
        {
            double[,] array = new double[aCentroidMatrix.Rows, 3];
            for (int i = 0; i < aCentroidMatrix.Rows; i++)
            {
                array[i, 0] = aCentroidMatrix.ElementAt(i).X;
                array[i, 1] = aCentroidMatrix.ElementAt(i).Y;
                array[i, 2] = aCentroidMatrix.ElementAt(i).Z;
            }
            return array;
        }

        private static List<Vector3> CreateVector3Array(MatOfPoint3f origPoint3f)
        {
            var list = new List<Vector3>();
            foreach (var p in origPoint3f)
            {
                list.Add(new Vector3(p.X, p.Y, p.Z));
            }
            return list;
        }

        private static Mat ConvertToFC1(MatOfPoint3f orig)
        {
            Point3f[] pts = new Point3f[orig.Cols * orig.Rows];
            orig.CopyTo(pts, 0);
            Mat m = new Mat(orig.Rows, 3, MatType.CV_32FC1);
            for (int i = 0; i < orig.Rows; i++)
            {
                var pt = orig.ElementAt(i);
                m.SetArray(0, 0, pt.X);
                m.SetArray(0, 1, pt.Y);
                m.SetArray(0, 2, pt.Z);
            }
            return m;
        }

        public static MatOfPoint3f GetCentroid(MatOfPoint3f orig)
        {
            int i = 0;
            var centroid = new Point3f();
            foreach (var p in orig)
            {
                centroid += p;
                i++;
            }
            var point = new Point3f(centroid.X / i, centroid.Y / i, centroid.Z / i);
            var mat = new MatOfPoint3f();
            mat.Add(point);
            return mat;
        }
    }
}
