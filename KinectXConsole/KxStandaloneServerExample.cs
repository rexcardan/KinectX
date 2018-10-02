using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class KxStandaloneServerExample
    {
        public static void Run(string[] args)
        {
            KinectX.Network.KxServer.Start();
            Console.ReadLine();
        }
    }
}
