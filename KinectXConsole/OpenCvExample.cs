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
    public class OpenCvExample
    {
        public static void Run(string[] args)
        {
            var xef = new Xef(@"../../../Resources/cube.xef");
            var depth = xef.LoadDepthFrame(0);
            var color = xef.LoadColorFrame(0);
            var cvColor = new CvColor(color);
            var cvDepth = new CvDepth(depth);
            cvColor.Show();
            Console.Read();
        }
    }
}
