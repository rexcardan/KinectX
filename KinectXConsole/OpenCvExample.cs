using KinectX.Data;
using KinectX.Extensions;
using KinectX.IO;
using System;

namespace KinectXConsole
{
    public class OpenCvExample
    {
        public static void Run(string[] args)
        {
            var xef = new Xef(@"../../../Resources/cube.xef");
            var depth = xef.LoadDepthFrame(0);
            var color = xef.LoadColorFrame(0);
            var cvColor = new CvColor(color);
            var cvDepth = new CvDepth(depth);
            //render kinect color to UI (using KinectX.Extensions;)
            cvColor.Show();
            Console.Read();
        }
    }
}
