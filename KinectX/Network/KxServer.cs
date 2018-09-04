using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Network
{
    public class KxServer : IKxServer
    {
        public long LastColorExposureTimeTicks()
        {
            throw new NotImplementedException();
        }

        public Task<long> LastColorExposureTimeTicksAsync()
        {
            throw new NotImplementedException();
        }

        public float LastColorGain()
        {
            throw new NotImplementedException();
        }

        public Task<float> LastColorGainAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] LatestDepthImage()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> LatestDepthImageAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] LatestJPEGImage()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> LatestJPEGImageAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] LatestRGBImage()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> LatestRGBImageAsync()
        {
            throw new NotImplementedException();
        }

        public byte[] LatestYUVImage()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> LatestYUVImageAsync()
        {
            throw new NotImplementedException();
        }
    }
}
