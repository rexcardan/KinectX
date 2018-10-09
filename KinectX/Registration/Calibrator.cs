using KinectX.Data;
using KinectX.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Registration
{
    public class Calibrator
    {
        public static (KxTransform Transform, List<Marker> Markers) Calibrate(CvColor cvColor, CvCameraSpace cs)
        {
            //Define Board
            var cube = CoordinateDefinition.Cube();
            //Look for Board
            var markers = Vision.FindAruco(cvColor);
            if (!markers.Any()) { throw new Exception("No calibration pattern could be found in the image!"); }
            //Calculate Camera Pose
            return (Vision.GetPoseFromImage(cube, cs, markers), markers);
        }
    }
}
