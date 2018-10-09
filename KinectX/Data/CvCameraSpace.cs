using KinectX.Meta;
using Microsoft.Kinect;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Data
{
    public class CvCameraSpace : Mat
    {
        public CvCameraSpace(CameraSpacePoint[] cps) : base(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_32FC3)
        {
            var floats = cps.SelectMany(c => new float[] { c.X, c.Y, c.Z }).ToArray();
            Marshal.Copy(floats, 0, Data, floats.Length);
        }

        public CvCameraSpace() : base(0, 1, MatType.CV_32FC3)
        {

        }

        private unsafe CvCameraSpace(Mat m) : base(m.Rows, m.Cols, m.Type())
        {
            m.CopyTo(this);
        }

        public void Add(Point3f point)
        {
            var mat = new Mat(1, 1, MatType.CV_32FC3, new Scalar(point.X, point.Y, point.Z));
            base.Add(mat);
        }


        public CameraSpacePoint[] GetPoints()
        {
            var vals = new Point3f[KinectSettings.COLOR_PIXEL_COUNT];
            GetArray(0, 0, vals);
            return vals.Select(v => new CameraSpacePoint() { X = v.X, Y = v.Y, Z = v.Z }).ToArray();
        }

        /// <summary>
        /// Mask for values which are real locations
        /// </summary>
        public Mat GetRealMask()
        {
            return InRange(new MatOfFloat(1, 1, float.MinValue), new MatOfFloat(1, 1, float.MaxValue));
        }

        /// <summary>
        /// Mask for values which are inf, inf, inf or NaN, Nan, Nan
        /// </summary>
        /// <returns></returns>
        public Mat GetIndeterminteMask()
        {
            var mask = new Mat();
            Cv2.BitwiseNot(GetRealMask(), mask);
            return mask;
        }

        //public new CvCameraSpace SubMat(Rect rect)
        //{
        //    using (var mat = base.SubMat(rect))
        //    {
        //        return new CvCameraSpace(mat);
        //    }
        //}
    }
}
