using KinectX.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class RemoteKinectXEFExample
    {
        public static void Run(string[] args)
        {
            var servers = KxServerFinder.FindServers();
            foreach (var s in servers)
            {
                var name = s.Endpoint.Address.Uri.DnsSafeHost;
                var success = s.RecordXef(TimeSpan.FromMilliseconds(1000));
                var xef = s.LastRecording();
                //Store xef file locally
                File.WriteAllBytes($"{name}.xef", xef);
            }
        }
    }
}
