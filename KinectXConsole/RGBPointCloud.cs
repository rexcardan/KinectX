using KinectX.Extensions;
using KinectX.IO;
using KinectX.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class RGBPointCloud
    {
        public static void Run()
        {
            var xefPath = @"C:\XEF\cam1_cal.xef";
            var xef = new Xef(xefPath);
            //Load computer vision (CV) color file
            var colorCV = xef.LoadCvColorFrame(0);
            var cameraSpace = xef.LoadCVCameraSpace(0);
            var pose = Calibrator.Calibrate(colorCV, cameraSpace)
                .Transform
                .CameraSpaceToWorldTx
                .ToMat();
            cameraSpace.Transform(pose);
                //Save as XYZRGB file (open in MeshLab to view)
            XYZRGB.Export(cameraSpace.ToCamSpacePoints(), colorCV.GetBRGABytes(), @"C:\XEF\cam1_cal.txt");
        }
    }
}
