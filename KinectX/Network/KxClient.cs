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
    public class KxClient : System.ServiceModel.ClientBase<IKxServer>, IKxServer
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

        public byte[] LatestDepthImage()
        {
            return base.Channel.LatestDepthImage();
        }

        public System.Threading.Tasks.Task<byte[]> LatestDepthImageAsync()
        {
            return base.Channel.LatestDepthImageAsync();
        }

        public byte[] LatestYUVImage()
        {
            return base.Channel.LatestYUVImage();
        }

        public System.Threading.Tasks.Task<byte[]> LatestYUVImageAsync()
        {
            return base.Channel.LatestYUVImageAsync();
        }

        public byte[] LatestRGBImage()
        {
            return base.Channel.LatestRGBImage();
        }

        public System.Threading.Tasks.Task<byte[]> LatestRGBImageAsync()
        {
            return base.Channel.LatestRGBImageAsync();
        }

        public byte[] LatestJPEGImage()
        {
            return base.Channel.LatestJPEGImage();
        }

        public System.Threading.Tasks.Task<byte[]> LatestJPEGImageAsync()
        {
            return base.Channel.LatestJPEGImageAsync();
        }

        public float LastColorGain()
        {
            return base.Channel.LastColorGain();
        }

        public System.Threading.Tasks.Task<float> LastColorGainAsync()
        {
            return base.Channel.LastColorGainAsync();
        }

        public long LastColorExposureTimeTicks()
        {
            return base.Channel.LastColorExposureTimeTicks();
        }

        public System.Threading.Tasks.Task<long> LastColorExposureTimeTicksAsync()
        {
            return base.Channel.LastColorExposureTimeTicksAsync();
        }

        //public RoomAliveToolkit.Kinect2Calibration GetCalibration()
        //{
        //    return base.Channel.GetCalibration();
        //}

        //public System.Threading.Tasks.Task<RoomAliveToolkit.Kinect2Calibration> GetCalibrationAsync()
        //{
        //    return base.Channel.GetCalibrationAsync();
        //}
    }
}
