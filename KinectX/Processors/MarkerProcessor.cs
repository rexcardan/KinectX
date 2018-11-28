using KinectX.Data;
using KinectX.Extensions;
using KinectX.Registration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Processors
{
    public class MarkerProcessor
    {
        public static Point3f FindCenter(Marker marker, CvCameraSpace cs)
        {
            //Isolate camera space to just this marker
            var marker_3d = cs.SubMat(Cv2.BoundingRect(marker.Points));
            //Mask out pixels with bad (infinite) data
            var realMask = marker_3d.GetRealMask();
            marker.MaskSum = realMask.Sum();
            //Find the mean of all of the 3D points
            var scalarCenter = marker_3d.Mean();
            return new Point3f((float)scalarCenter.Val0, (float)scalarCenter.Val1, (float)scalarCenter.Val2);
        }

        public static Point3f FindLocation(Point2f corn, CvCameraSpace cs)
        {
            Mat patch =new Mat();
            Cv2.GetRectSubPix(cs, new Size(1, 1), corn, patch);
            return patch.At<Point3f>(0, 0);
        }
    }
}
