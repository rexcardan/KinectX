using KinectX.Data;
using KinectX.Extensions;
using KinectX.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class XEFOpenCvExample
    {
        public static void Run(string[] args)
        {
            var xef = new Xef(@"../../../Resources/cube.xef");
            //Load the first depth frame (frame 0)
            var depth = xef.LoadDepthFrame(0);
            //Load the first color frame (frame 0)
            var color = xef.LoadColorFrame(0);
            var cvColor = new CvColor(color);
            var cvDepth = new CvDepth(depth);
            //render kinect color to UI (using KinectX.Extensions;)
            cvColor.Show();
            Console.Read();
        }
    }
}
