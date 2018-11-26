using KinectX.Data;
using KinectX.Data.Listeners;
using KinectX.Extensions;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.IO;
using KinectX.Meta;
using KinectX.Registration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Pose = KinectX.Registration.XefPoseFinder;

namespace KinectXConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoteKinectXEFExample.Run(null);
            RGBPointCloudExampleWithMarkerHightlights.Run(null);
            MulticameraFusionExample.Run(null);

            var xefPath = @"C:\XEF\cam1_cal.xef";
            var xef = new Xef(xefPath);
            //Load computer vision (CV) color file
            var colorCV = xef.LoadCvColorFrame(0);
            var cube = CoordinateDefinition.Microcube();

            var markers = colorCV.FindAruco();
            var colorBytes = colorCV.GetBRGABytes();
            var cspace = xef.LoadCVCameraSpace(2);
            var marker1 = cspace.SubMat(Cv2.BoundingRect(markers.First().Points));
            var realMask = marker1.GetRealMask();
            var center = marker1.Mean(realMask);

            var marker2 = cspace.SubMat(Cv2.BoundingRect(markers.Skip(1).First().Points));
            realMask = marker2.GetRealMask();
            var center2 = marker2.Mean(realMask);
            var between = center2.DistanceTo(center);
            var indeterminiteMask = marker1.GetIndeterminteMask();
            realMask.ShowNoWait();
            Cv2.WaitKey(0);
            Console.Read();
            //Save as XYZRGB file (open in MeshLab to view)
           // XYZRGB.Export(cameraSpace, colorBytes, @"C:\XEF\cam1_cal.txt");
        }
    }
}
