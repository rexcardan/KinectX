using KinectX.Extensions;
using KinectX.IO;
using KinectX.Mathematics;
using KinectX.Meta;
using Microsoft.Kinect;
using NLog;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Registration
{
    public class PoseFinder
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static KxTransform GetPoseFromXef(string xefPath)
        {           
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Cube();
            //Find registration
            var xef = new Xef(xefPath);
            var colorCv = xef.LoadCvColorFrame(0);

            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            //Vision.DrawAruco(colorCv).Show();

            //Calculate pose
            var _3dImage = xef.LoadCameraSpace(5);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            return kxTransform;
        }

        public static List<float> ValidatePose(MatOfFloat pose, CoordinateDefinition def, List<Marker> markers)
        {
            List<float> deltas = new List<float>();
            markers.ForEach(m =>
            {
                if (def.ContainsCode(m.Id))
                {
                    for (int i = 0; i < m.Points.Length; i++)
                    {
                        var realPos = def.CornerDefinitions[m.Id][i];
                        var kPos = m.KinectPositions[i];
                        var ptTx = pose.TransformPoint3f(kPos);
                        var delta = (ptTx - realPos).Magnitude();
                        deltas.Add(delta);
                    }
                }
            });
            return deltas;
        }

        /// <summary>
        /// Looks up the 2D point in color space to the corresponding 3D point from the camera space map.
        /// Also interpolates in 3D space if 2D point is not an integer pixel  (often the case)
        /// </summary>
        /// <param name="camPoints">the 3D camera space mapped to color coordinates (provided by Kinect API)</param>
        /// <param name="pt2D">a 2D lookup point in color space</param>
        /// <returns>the cooresponding 3D point in camera space at the 2D lookup position</returns>
        private static Point3f GetKinectPosition(CameraSpacePoint[] camPoints, Point2f pt2D)
        {
            var fx = pt2D.X;
            var fy = pt2D.Y;

            //Since fiducial is likely not exactly on an integer pixel xy, interpolate by forming
            //a vector basis of surrounding pixels
            var toCamPoint = new Func<double, double, Point3f>((x, y) =>
            {
                var index = (int)Math.Round(y) * KinectSettings.COLOR_WIDTH + (int)Math.Round(x);
                var camMatch = camPoints[index];
                return new Point3f(camMatch.X, camMatch.Y, camMatch.Z);
            });

            var p1 = toCamPoint(Math.Floor(fx), Math.Floor(fy));
            var p2 = toCamPoint(Math.Floor(fx), Math.Ceiling(fy));
            var p3 = toCamPoint(Math.Ceiling(fx), Math.Floor(fy));

            var xBasis = p3 - p1;
            var yBasis = p2 - p1;

            var xBasisWeight = fx - (float)Math.Truncate(fx);
            var yBasisWeight = fy - (float)Math.Truncate(fy);

            var point = p1 + new Point3f(xBasis.X * xBasisWeight, xBasis.Y * xBasisWeight, xBasis.Z * (xBasisWeight))
                + new Point3f(yBasis.X * yBasisWeight, yBasis.Y * yBasisWeight, yBasis.Z * (yBasisWeight));
            return point;
        }
    }
}
