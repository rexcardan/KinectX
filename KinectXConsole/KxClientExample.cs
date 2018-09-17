using KinectX.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class KxClientExample
    {
        /// <summary>
        /// Need a server running first (See KxStandaloneServerExample.cs)
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            var cameras = KxServerFinder.FindServers();
            Console.WriteLine(cameras.Count.ToString());
            foreach (var cam in cameras)
            {
                var depth = cam.LatestDepthImage();
                //Do something
            }
        }
    }
}
