using KinectX.Data;
using KinectX.Data.Listeners;
using KinectX.Extensions;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.IO;
using KinectX.Network;
using KinectX.Registration;
using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectXConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ks = new KxStream())
            {
                var color = ks.LatestRGBImage();
                var cvColor = CvColor.FromBGR(color);
                var depth = ks.LatestDepthImage();
                cvColor.Show();
            }
            Console.Read();
        }
    }
}
