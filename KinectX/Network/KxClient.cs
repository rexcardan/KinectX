using KinectX.Network.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Network
{
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public class KxClient : System.ServiceModel.ClientBase<KinectX.Network.Interfaces.KxServer>, KinectX.Network.Interfaces.KxServer
    {
        public static KxClient GenerateClient(EndpointAddress address)
        {
            var binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = 8295424;
            binding.Security.Mode = SecurityMode.None;
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


        //public KinectX.Network.Kinect2Calibration GetCalibration()
        //{
        //    return base.Channel.GetCalibration();
        //}

        //public System.Threading.Tasks.Task<KinectX.Network.Kinect2Calibration> GetCalibrationAsync()
        //{
        //    return base.Channel.GetCalibrationAsync();
        //}
    }
}
