using System;
using System.ServiceModel;

namespace KinectX.Network
{
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public class KxClient : System.ServiceModel.ClientBase<KinectX.Network.Interfaces.KxServer>, KinectX.Network.Interfaces.KxServer
    {
        public static KxClient GenerateClient(EndpointAddress address)
        {
            var binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = 2147483647;
            binding.Security.Mode = SecurityMode.None;
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.CloseTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);

            var client = new KxClient(binding, address);
            try
            {
                client.Open();
            }
            catch (EndpointNotFoundException e)
            {
                client = null;
                Console.WriteLine("could not connect to Kinect server '{0}'", address);
                throw e;
            }
            return client;
        }

        private KxClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        public bool IsCameraAvailable()
        {
            return base.Channel.IsCameraAvailable();
        }


        public ushort[] LatestDepthImage()
        {
            return base.Channel.LatestDepthImage();
        }


        public byte[] LatestYUVImage()
        {
            return base.Channel.LatestYUVImage();
        }


        public byte[] LatestRGBImage()
        {
            return base.Channel.LatestRGBImage();
        }


        public byte[] LatestJPEGImage()
        {
            return base.Channel.LatestJPEGImage();
        }


        public float LastColorGain()
        {
            return base.Channel.LastColorGain();
        }


        public long LastColorExposureTimeTicks()
        {
            return base.Channel.LastColorExposureTimeTicks();
        }

        public byte[] FusionCameraPose()
        {
            return base.Channel.FusionCameraPose();
        }

        public byte[] CameraPose()
        {
            return base.Channel.CameraPose();
        }

        public bool RecordXef(TimeSpan duration)
        {
            return base.Channel.RecordXef(duration);
        }

        public byte[] LastRecording()
        {
            return base.Channel.LastRecording();
        }
    }
}
