using KinectX.Meta;
using KinectX.Network;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectX.Network
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceContract]
    public class KxServer
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        private ushort[] depthShortBuffer = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
        private byte[] yuvByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 2];
        private byte[] rgbByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];
        private AutoResetEvent depthFrameReady = new AutoResetEvent(false);
        private AutoResetEvent yuvFrameReady = new AutoResetEvent(false);
        private AutoResetEvent rgbFrameReady = new AutoResetEvent(false);
        private AutoResetEvent jpegFrameReady = new AutoResetEvent(false);
        private AutoResetEvent audioFrameReady = new AutoResetEvent(false);
        private Queue<byte[]> audioFrameQueue = new Queue<byte[]>();


        public KxServer()
        {
            _logger.Info("Constructing server...");
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
            _logger.Info("Closing channel...");
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

        [OperationContract]
        public long LastColorExposureTimeTicks()
        {
            return KxBuffer.instance.lastColorExposureTimeTicks;
        }


        [OperationContract]
        public float LastColorGain()
        {
            return KxBuffer.instance.lastColorGain;
        }


        [OperationContract]
        public ushort[] LatestDepthImage()
        {
            _logger.Info("Getting latest depth image...");
            try
            {
                this.depthFrameReady.WaitOne();
                lock (KxBuffer.instance.depthShortBuffer)
                    Buffer.BlockCopy((Array)KxBuffer.instance.depthShortBuffer, 0, (Array)this.depthShortBuffer, 0, KinectSettings.DEPTH_PIXEL_COUNT * 2);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return this.depthShortBuffer;
        }


        [OperationContract]
        public byte[] LatestJPEGImage()
        {
            throw new NotImplementedException();
        }


        [OperationContract]
        public byte[] LatestRGBImage()
        {
            this.rgbFrameReady.WaitOne();
            lock (KxBuffer.instance.rgbByteBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.rgbByteBuffer, 0, (Array)this.rgbByteBuffer, 0, KinectSettings.COLOR_PIXEL_COUNT * 4);
            return this.rgbByteBuffer;
        }


        [OperationContract]
        public byte[] LatestYUVImage()
        {
            throw new NotImplementedException();
        }


        [OperationContract]
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

        public static void Start()
        {
            _logger.Info("Starting Kinect service...");
            new KxBuffer();
            ServiceHost serviceHost = new ServiceHost(typeof(KxServer));
            serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
            serviceHost.Open();
            _logger.Info("Kinect listener service!");
            Console.ReadLine();
        }
    }
}
