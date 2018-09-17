using KinectX.Help;
using KinectX.Meta;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Registration
{
    public class Vision
    {
        public static List<Marker> FindAruco(Mat m)
        {
            //Kinect color image is flipped, correct before ARUCO detection
            var flipped = m.Flip(FlipMode.Y);
            List<Marker> markers = new List<Marker>();
            var dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict6X6_250);
            Point2f[][] points;
            Point2f[][] rejPoints;
            int[] ids;
            var parms = DetectorParameters.Create();
            CvAruco.DetectMarkers(flipped, dictionary, out points, out ids, parms, out rejPoints);
            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var arucoMarkerCorners = points[i];

                for (int j = 0; j < arucoMarkerCorners.Length; j++)
                {
                    arucoMarkerCorners[j].X = KinectSettings.COLOR_WIDTH - arucoMarkerCorners[j].X - 1;
                }

                markers.Add(new Marker() { Id = id, Points = arucoMarkerCorners });
            }
            return markers;
        }

        public static void DrawAruco(Mat image)
        {
            var aruco = FindAruco(image);
            CvAruco.DrawDetectedMarkers(image,
                aruco.Select(a => a.Points).ToArray(),
                aruco.Select(a => a.Id).ToArray());
        }
    }
}
