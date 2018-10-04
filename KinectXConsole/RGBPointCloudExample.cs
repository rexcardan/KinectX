using KinectX.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class RGBPointCloudExample
    {
        public static void Main(string[] args)
        {
            var xefPath = @"C:\XEF\cam1_cal.xef";
            var xef = new Xef(xefPath);
            //Load computer vision (CV) color file
            var colorCV = xef.LoadCvColorFrame(0);
            var colorBytes = colorCV.GetBRGABytes();
            var cameraSpace = xef.LoadCameraSpace(2);
            //Save as XYZRGB file (open in MeshLab to view)
            XYZRGB.Export(cameraSpace, colorBytes, @"C:\XEF\cam1_cal.txt");
        }
    }
}
