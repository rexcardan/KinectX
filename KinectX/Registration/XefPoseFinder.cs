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
    public class XefPoseFinder
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static KxTransform GetPoseFromXef(string xefPath)
        {           
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Microcube();
            //Find registration
            var xef = new Xef(xefPath);
            var colorCv = xef.LoadCvColorFrame(0);
            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            //Vision.DrawAruco(colorCv).Show();

            //Calculate pose
            var _3dImage = xef.LoadCVCameraSpace(5);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            return kxTransform;
        }
    }
}
