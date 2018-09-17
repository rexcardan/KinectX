using KinectX.Mathematics;
using KinectX.Extensions;
using Microsoft.Kinect;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using KinectX.Meta;

namespace KinectX.Registration
{
    public class PoseFinder
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Calculates the camera pose (Kinect space to real space transform) based on coordinate definition
        /// and detected markers
        /// </summary>
        /// <param name="def">the definition of the visible coordinates</param>
        /// <param name="_3dImage">the 3d points mapped to Kinect color coordinates</param>
        /// <param name="markers">the detected markers in the color image</param>
        /// <returns>a 4x4 matrix of the camera pose</returns>
        public static MatOfFloat GetPoseFromImage(CoordinateDefinition def, CameraSpacePoint[] _3dImage, List<Marker> markers)
        {
            MatOfPoint3f sourcePts = new MatOfPoint3f();
            MatOfPoint3f destPts = new MatOfPoint3f();
            MatOfPoint3f transformedTest = new MatOfPoint3f();

            if (markers != null)
            {
                //For each marker found, look up Kinect position (2D -> 3D), find corresponding real position
                //Add KPos and RealPos to two arrays to calculate the transform between
                markers.ForEach(m =>
                {
                    if (def.ContainsCode(m.Id))
                    {
                        var kPositions = new List<Point3f>();
                        //Order of pts is Top Left, Top Right, Bottom Right, Bottom Left
                        for (int i = 0; i < m.Points.Length; i++) // Length will be 4
                        {
                            var kPos = GetKinectPosition(_3dImage, m.Points[i]);
                            kPositions.Add(kPos);
                            if (!double.IsNaN(kPos.X)) // then all not Nan
                            {
                                var realPos = def.CornerDefinitions[m.Id][i];
                                //Source is Kinect position
                                sourcePts.Add(kPos);
                                //Destination is actual physical location
                                destPts.Add(realPos);
                            }
                        }
                        //Add kinect positions to marker object
                        m.KinectPositions = kPositions.ToArray();
                    }
                });

                _logger.Info($"Using {sourcePts.Count / 4} markers to find pose...");

                //Need to flip Z to get into DICOM coordinates
                if (!sourcePts.Any() || !destPts.Any())
                {
                    throw new Exception("No points to transform!");
                }
                var txArray = new float[4, 4];
                var tx = Transform.TransformBetween(sourcePts, destPts);

                //Validate Pose
                //Validate that the transforms are valid...Low average and low STD desired
                var deltas = PoseFinder.ValidatePose(tx, def, markers);
                var avgDelta = deltas.Average();
                var std = deltas.StdDev();
                _logger.Info($"Pose calculated with average delta of : ");
                _logger.Info($"{(avgDelta*1000).ToString("N3")} ± {(std*1000).ToString("N3")} mm");
                return tx;
            }
            throw new ArgumentException("Markers cannot be null.");
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
