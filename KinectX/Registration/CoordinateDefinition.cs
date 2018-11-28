using KinectX.Extensions;
using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Registration
{
    /// <summary>
    /// This class holds the relationship between codes and the 3D positions of its corners. It assumes all visual codes
    /// are square. This allows different "boards" with different fidcucial code placements to be recognized.
    /// </summary>
    public class CoordinateDefinition
    {
        /// <summary>
        /// A dictionary containing the fiducial ids with a list of the corner 3D coordinates. Coordinates are stored in meters
        /// </summary>
        public Dictionary<int, List<Point3f>> CornerDefinitions { get; private set; } = new Dictionary<int, List<Point3f>>();

        /// <summary>
        /// A dictionary containing the fiducial ids with a list of the center (of the marker) 3D coordinates. Coordinates are stored in meters
        /// </summary>
        public Dictionary<int, Point3f> CenterDefinitions { get; private set; } = new Dictionary<int, Point3f>();

        /// <summary>
        /// Returns whether the input code is present in this code defition. Used to filter unknown codes
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool ContainsCode(int code)
        {
            return CenterDefinitions.ContainsKey(code);
        }

        /// <summary>
        /// Adds a fiducial to the known fiducials of this board
        /// </summary>
        /// <param name="id">the id or code of the fiducial</param>
        /// <param name="topL">This is the top left corner (highest y value and lowest x) of this fiducial </param>
        /// <param name="width">the width of the fiducial square pattern in meters</param>
        public void Add(int id, Point3f topL, float width, Point3f xDir, Point3f yDir)
        {
            var nx = xDir.Normalize() * width;
            var ny = yDir.Normalize() * width;

            var corners = new List<Point3f>();
            var topR = new Point3f(topL.X + nx.X, topL.Y + nx.Y, topL.Z + nx.Z);
            var bottomR = new Point3f(topL.X + nx.X + ny.X, topL.Y + nx.Y + ny.Y, topL.Z + nx.Z + ny.Z);
            var bottomL = new Point3f(topL.X + ny.X, topL.Y + ny.Y, topL.Z + ny.Z);


            corners.Add(topL);
            corners.Add(topR);
            corners.Add(bottomR);
            corners.Add(bottomL);
            var center = new Point3f(corners.Average(c => c.X), corners.Average(c => c.Y), corners.Average(c => c.Z));
            CenterDefinitions.Add(id, center);
            CornerDefinitions.Add(id, corners);
        }


        public static CoordinateDefinition Microcube()
        {
            var cubeDepth = 0.052f;
            var markerFar = 0.0475f;
            var markerClose = 0.0025f;
            var width = 0.045f;

            var bd = new CoordinateDefinition();
            //Side 1 (1,2,3,4) - Proximal Cube Face
            bd.CenterDefinitions.Add(0, new Point3f(0.025f, 0.025f, -cubeDepth));
            bd.CenterDefinitions.Add(1, new Point3f(-0.025f, 0.025f, -cubeDepth));
            bd.CenterDefinitions.Add(2, new Point3f(0.025f, -0.025f, -cubeDepth));
            bd.CenterDefinitions.Add(3, new Point3f(-0.025f, -0.025f, -cubeDepth));

            //Side 2 (4,5,6,7) - Right Cube Face
            bd.CenterDefinitions.Add(4, new Point3f(-cubeDepth, 0.025f, -0.025f));
            bd.CenterDefinitions.Add(5, new Point3f(-cubeDepth, 0.025f, 0.025f));
            bd.CenterDefinitions.Add(6, new Point3f(-cubeDepth, -0.025f, -0.025f));
            bd.CenterDefinitions.Add(7, new Point3f(-cubeDepth, -0.025f, 0.025f));

            //Side 3 (8,9,10,11) - Distal Cube Face
            bd.CenterDefinitions.Add(8, new Point3f(-0.025f, 0.025f, cubeDepth));
            bd.CenterDefinitions.Add(9, new Point3f(0.025f, 0.025f, cubeDepth));
            bd.CenterDefinitions.Add(10, new Point3f(-0.025f, -0.025f, cubeDepth));
            bd.CenterDefinitions.Add(11, new Point3f(0.025f, -0.025f, cubeDepth));

            //Side 4 (12,13,14,15) - Left Cube Face
            bd.CenterDefinitions.Add(12, new Point3f(cubeDepth, 0.025f, 0.025f));
            bd.CenterDefinitions.Add(13, new Point3f(cubeDepth, 0.025f, -0.025f));
            bd.CenterDefinitions.Add(14, new Point3f(cubeDepth, -0.025f, 0.025f));
            bd.CenterDefinitions.Add(15, new Point3f(cubeDepth, -0.025f, -0.025f));

            //Side 5  - Top Cube Face
            bd.CenterDefinitions.Add(16, new Point3f(0.025f, cubeDepth, 0.025f));
            bd.CenterDefinitions.Add(17, new Point3f(-0.025f, cubeDepth, 0.025f));
            bd.CenterDefinitions.Add(18, new Point3f(0.025f, cubeDepth, -0.025f));
            bd.CenterDefinitions.Add(19, new Point3f(-0.025f, cubeDepth, -0.025f));
            return bd;
        }

        public static CoordinateDefinition Cube(double cubeDepthCM, double squareWidthCM)
        {
            var pixelsPerCM = 300 / 2.54;
            var cmWidth = squareWidthCM * 10.4 / 4.5;
            var paperPxWidth = pixelsPerCM * cmWidth; //11in - 1in margin *300px/in 
            var paperPxHeight = pixelsPerCM * cmWidth; //17in - 1in margin *300px/in
            var markerPxWidth = (int)(pixelsPerCM * 4.5 / 10.4 * cmWidth);
            var marginWidthPx = (int)((paperPxWidth - 2.0 * markerPxWidth) / 4);
            var marginHeight = marginWidthPx;

            var markerFar = (float)((markerPxWidth + marginWidthPx) / pixelsPerCM);
            var markerClose = (float)(marginWidthPx / pixelsPerCM);
            float cubeDepth = (float)cubeDepthCM / 2.0f;
            var width = (float)squareWidthCM;

            var bd = new CoordinateDefinition();
            //Side 1 (1,2,3,4) - Proximal Cube Face
            bd.Add(0, new Point3f(markerFar, markerFar, -cubeDepth),
                width, new Point3f(-1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(1, new Point3f(-markerClose, markerFar, -cubeDepth),
               width, new Point3f(-1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(2, new Point3f(markerFar, -markerClose, -cubeDepth),
               width, new Point3f(-1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(3, new Point3f(-markerClose, -markerClose, -cubeDepth),
               width, new Point3f(-1, 0, 0), new Point3f(0, -1, 0));

            //Side 2 (4,5,6,7) - Right Cube Face
            bd.Add(4, new Point3f(-cubeDepth, markerFar, -markerFar),
                width, new Point3f(0, 0, 1), new Point3f(0, -1, 0));
            bd.Add(5, new Point3f(-cubeDepth, markerFar, markerClose),
               width, new Point3f(0, 0, 1), new Point3f(0, -1, 0));
            bd.Add(6, new Point3f(-cubeDepth, -markerClose, -markerFar),
               width, new Point3f(0, 0, 1), new Point3f(0, -1, 0));
            bd.Add(7, new Point3f(-cubeDepth, -markerClose, markerClose),
               width, new Point3f(0, 0, 1), new Point3f(0, -1, 0));

            //Side 3 (8,9,10,11) - Distal Cube Face
            bd.Add(8, new Point3f(-markerFar, markerFar, cubeDepth),
                 width, new Point3f(1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(9, new Point3f(markerClose, markerFar, cubeDepth),
               width, new Point3f(1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(10, new Point3f(-markerFar, -markerClose, cubeDepth),
               width, new Point3f(1, 0, 0), new Point3f(0, -1, 0));
            bd.Add(11, new Point3f(markerClose, -markerClose, cubeDepth),
               width, new Point3f(1, 0, 0), new Point3f(0, -1, 0));

            //Side 4 (12,13,14,15) - Left Cube Face
            bd.Add(12, new Point3f(cubeDepth, markerFar, markerFar),
                width, new Point3f(0, 0, -1), new Point3f(0, -1, 0));
            bd.Add(13, new Point3f(cubeDepth, markerFar, -markerClose),
               width, new Point3f(0, 0, -1), new Point3f(0, -1, 0));
            bd.Add(14, new Point3f(cubeDepth, -markerClose, markerFar),
               width, new Point3f(0, 0, -1), new Point3f(0, -1, 0));
            bd.Add(15, new Point3f(cubeDepth, -markerClose, -markerClose),
               width, new Point3f(0, 0, -1), new Point3f(0, -1, 0));

            //Side 5  - Top Cube Face
            bd.Add(16, new Point3f(markerFar, cubeDepth, markerFar),
                width, new Point3f(-1, 0, 0), new Point3f(0, 0, -1));
            bd.Add(17, new Point3f(-markerClose, cubeDepth, markerFar),
               width, new Point3f(-1, 0, 0), new Point3f(0, 0, -1));
            bd.Add(18, new Point3f(markerFar, cubeDepth, markerClose),
              width, new Point3f(-1, 0, 0), new Point3f(0, 0, -1));
            bd.Add(19, new Point3f(-markerClose, cubeDepth, markerClose),
              width, new Point3f(-1, 0, 0), new Point3f(0, 0, -1));

            return bd;
        }

    }
}


