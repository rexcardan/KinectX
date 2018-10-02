using NLog;
using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;

namespace KinectX.Network
{
    public class KxServerFinder
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static List<KxClient> FindServers()
        {
            List<KxClient> clients = new List<KxClient>();
            var discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            var findCriteria = new FindCriteria(typeof(KxServer));
            findCriteria.Duration = new TimeSpan(0, 0, 2);
            _logger.Info("Finding Kinect servers...");
            var services = discoveryClient.Find(findCriteria);
            discoveryClient.Close();
            Console.WriteLine("Found {0} servers", services.Endpoints.Count);
            foreach (var endPoint in services.Endpoints)
            {
                Console.WriteLine(endPoint.Address);
               clients.Add(KxClient.GenerateClient(endPoint.Address));
            }
            return clients;
        }
    }
}
