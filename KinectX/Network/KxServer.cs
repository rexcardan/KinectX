using KinectX.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectX.Network
{
    public class KxServer : IKxServer
    {
        private byte[] depthByteBuffer = new byte[KinectSettings.DepthPixelCount * 2];
        private byte[] yuvByteBuffer = new byte[KinectSettings.ColorPixelCount * 2];
        private byte[] rgbByteBuffer = new byte[KinectSettings.ColorPixelCount * 4];
        private AutoResetEvent depthFrameReady = new AutoResetEvent(false);
        private AutoResetEvent yuvFrameReady = new AutoResetEvent(false);
        private AutoResetEvent rgbFrameReady = new AutoResetEvent(false);
        private AutoResetEvent jpegFrameReady = new AutoResetEvent(false);
        private AutoResetEvent audioFrameReady = new AutoResetEvent(false);
        private Queue<byte[]> audioFrameQueue = new Queue<byte[]>();

        private KxBuffer _buffer;

        public KxServer()
        {
            _buffer = new KxBuffer();
            lock (KxBuffer.instance.depthFrameReady)
                KxBuffer.instance.depthFrameReady.Add(this.depthFrameReady);
            lock (KxBuffer.instance.yuvFrameReady)
                KxBuffer.instance.yuvFrameReady.Add(this.yuvFrameReady);
            lock (KxBuffer.instance.rgbFrameReady)
                KxBuffer.instance.rgbFrameReady.Add(this.rgbFrameReady);
            lock (KxBuffer.instance.audioFrameReady)
                KxBuffer.instance.audioFrameReady.Add(this.audioFrameReady);
            lock (KxBuffer.instance.audioFrameQueues)
                KxBuffer.instance.audioFrameQueues.Add(this.audioFrameQueue);
            OperationContext.Current.Channel.Closed += Channel_Closed;
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            lock (KxBuffer.instance.depthFrameReady)
                KxBuffer.instance.depthFrameReady.Remove(this.depthFrameReady);
            lock (KxBuffer.instance.yuvFrameReady)
                KxBuffer.instance.yuvFrameReady.Remove(this.yuvFrameReady);
            lock (KxBuffer.instance.rgbFrameReady)
                KxBuffer.instance.rgbFrameReady.Remove(this.rgbFrameReady);
            lock (KxBuffer.instance.audioFrameReady)
                KxBuffer.instance.audioFrameReady.Remove(this.audioFrameReady);
            lock (KxBuffer.instance.audioFrameQueues)
                KxBuffer.instance.audioFrameQueues.Remove(this.audioFrameQueue);
        }

        public long LastColorExposureTimeTicks()
        {
            return KxBuffer.instance.lastColorExposureTimeTicks;
        }

        public async Task<long> LastColorExposureTimeTicksAsync()
        {
            return await Task.Run(() => { return KxBuffer.instance.lastColorExposureTimeTicks; });
        }

        public float LastColorGain()
        {
            return KxBuffer.instance.lastColorGain;
        }

        public async Task<float> LastColorGainAsync()
        {
            return await Task.Run(() => { return KxBuffer.instance.lastColorGain; });
        }

        public byte[] LatestDepthImage()
        {
            this.depthFrameReady.WaitOne();
            lock (KxBuffer.instance.depthShortBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.depthShortBuffer, 0, (Array)this.depthByteBuffer, 0, KinectSettings.DepthPixelCount * 2);
            return this.depthByteBuffer;
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
            this.rgbFrameReady.WaitOne();
            lock (KxBuffer.instance.rgbByteBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.rgbByteBuffer, 0, (Array)this.rgbByteBuffer, 0, KinectSettings.ColorPixelCount * 4);
            return this.rgbByteBuffer;
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

        public byte[] LatestAudio()
        {
            this.audioFrameReady.WaitOne();
            lock (KxBuffer.instance.audioFrameQueues)
            {
                byte[] numArray = new byte[this.audioFrameQueue.Count * 1024];
                int count = this.audioFrameQueue.Count;
                for (int index = 0; index < count; ++index)
                    Array.Copy((Array)this.audioFrameQueue.Dequeue(), 0, (Array)numArray, 1024 * index, 1024);
                return numArray;
            }
        }
    }
}
