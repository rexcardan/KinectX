using KinectX.Data;
using KinectX.Extensions;
using KinectX.IO;
using KinectX.Mathematics;
using KinectX.Registration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class RGBPointCloudExampleWithMarkerHightlights
    {
        public static void Run(string[] args)
        {
            var xefPath = @"C:\XEF\cam2_cal.xef";
            var xef = new Xef(xefPath);
            //Load computer vision (CV) color file
            var colorCV = xef.LoadCvColorFrame(0);
            colorCV.DrawAruco().ShowNoWait() ;
            var cameraSpace = xef.LoadCVCameraSpace(2);

            var (tx, markers) = Calibrator.Calibrate(colorCV, cameraSpace);
            var pose = tx
                .CameraSpaceToWorldTx
                .ToMat();

            var camSpaceTx = cameraSpace.Transform(pose)
                .ToCamSpacePoints();
            //Save as XYZRGB file (open in MeshLab to view)
            XYZRGB.Export(camSpaceTx, colorCV.GetBRGABytes(), @"C:\XEF\cam2_cal.txt");
            markers = markers.OrderByDescending(m => m.MaskSum.Val0).Take(4).ToList();
            var markerPoints = new CvCameraSpace();
            markers.ForEach(m => markerPoints.Add(m.KxCenter));
            var txMarkers = markerPoints.Transform(pose);

            XYZRGB.Export(txMarkers, new Scalar(255, 0, 0), @"C:\XEF\cam2_cal_markers.txt");

        }
    }
}
