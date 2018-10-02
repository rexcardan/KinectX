using System;

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
