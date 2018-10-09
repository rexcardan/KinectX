using KinectX.Data;
using KinectX.Extensions;
using KinectX.Mathematics;
using KinectX.Meta;
using KinectX.Processors;
using Microsoft.Kinect;
using NLog;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Registration
{
    public class Vision
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Finds ARUCO markers in an image and 
        /// </summary>
        /// <param name="colorCv"></param>
        /// <returns></returns>
        public static List<Marker> FindAruco(Mat colorCv)
        {
            Mat m = new Mat();
            colorCv.CopyTo(m);
            if (colorCv.Channels() == 4)
            {
                m = m.CvtColor(ColorConversionCodes.BGRA2BGR);
            }
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
                var area = Cv2.ContourArea(arucoMarkerCorners);
                markers.Add(new Marker() { Id = id, Points = arucoMarkerCorners, Area = area });
            }
            return markers.OrderByDescending(ma => ma.Area).ToList();
        }

        /// <summary>
        /// Calculates the camera pose (Kinect space to real space transform) based on coordinate definition
        /// and detected markers
        /// </summary>
        /// <param name="def">the definition of the visible coordinates</param>
        /// <param name="_3dImage">the 3d points mapped to Kinect color coordinates</param>
        /// <param name="markers">the detected markers in the color image</param>
        /// <returns>a 4x4 matrix of the camera pose</returns>
        public static KxTransform GetPoseFromImage(CoordinateDefinition def, CvCameraSpace cvcs, List<Marker> markers)
        {
            MatOfPoint3f sourcePts = new MatOfPoint3f();
            MatOfPoint3f destPts = new MatOfPoint3f();

            if (markers != null)
            {
                //For each marker found, look up Kinect position (2D -> 3D), find corresponding real position
                //Add KPos and RealPos to two arrays to calculate the transform between
                markers.ForEach(m =>
                {
                    if (def.ContainsCode(m.Id))
                    {
                        m.KxCenter = MarkerProcessor.FindCenter(m, cvcs);
                    }
                });

                //Todo: Take N best markers (highest mask sum means better 3d data)
                var ordered = markers.OrderByDescending(m => m.MaskSum.Val0).ToList();
                ordered.ForEach(m =>
                {
                    var realPos = def.CenterDefinitions[m.Id];
                    //Source is Kinect position
                    sourcePts.Add(m.KxCenter);
                    //Destination is actual physical location
                    destPts.Add(realPos);
                });

                _logger.Info($"Using {sourcePts.Count} markers to find pose...");

                //Need to flip Z to get into DICOM coordinates
                if (!sourcePts.Any() || !destPts.Any())
                {
                    throw new Exception("No points to transform!");
                }
                var txArray = new float[4, 4];
                var tx = Transform.TransformBetween(sourcePts, destPts);
                var kxTx = new KxTransform(tx.To2DArray());

                //Validate Pose
                //Validate that the transforms are valid...Low average and low STD desired
                var deltas = ValidatePose(tx, def, markers);
                var avgDelta = deltas.Average();
                var std = deltas.StdDev();
                _logger.Info($"Pose calculated with average delta of : ");
                _logger.Info($"{(avgDelta * 1000).ToString("N3")} ± {(std * 1000).ToString("N3")} mm");
                return kxTx;
            }
            throw new ArgumentException("Markers cannot be null.");
        }

        public static Mat DrawAruco(Mat colorCv)
        {
            Mat m = new Mat();
            colorCv.CopyTo(m);
            if (colorCv.Channels() == 4)
            {
                m = m.CvtColor(ColorConversionCodes.BGRA2BGR);
            }

            var aruco = FindAruco(m);
            CvAruco.DrawDetectedMarkers(m,
                aruco.Select(a => a.Points).ToArray(),
                aruco.Select(a => a.Id).ToArray());
            return m;
        }

        /// <summary>
        /// Applies the input transform in reverse of the KxPosition to real to determine
        /// the quality of the pose estimation.
        /// </summary>
        /// <param name="pose">the input pose 4x4 matrix</param>
        /// <param name="def">the definition of the ARUCO pattern in real space</param>
        /// <param name="markers">the markers found in the image</param>
        /// <returns>a list of deltas of each transformed center vs the real center</returns>
        public static List<float> ValidatePose(MatOfFloat pose, CoordinateDefinition def, List<Marker> markers)
        {
            List<float> deltas = new List<float>();
            markers.ForEach(m =>
            {
                if (def.ContainsCode(m.Id))
                {
                    var realPos = def.CenterDefinitions[m.Id];
                    var kPos = m.KxCenter;
                    var ptTx = pose.TransformPoint3f(kPos);
                    var delta = (ptTx - realPos).Magnitude();
                    deltas.Add(delta);

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
