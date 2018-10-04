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
            //Find the mean of all of the 3D points
            var scalarCenter = marker_3d.Mean();
            return new Point3f((float)scalarCenter.Val0, (float)scalarCenter.Val1, (float)scalarCenter.Val2);
        }
    }
}
