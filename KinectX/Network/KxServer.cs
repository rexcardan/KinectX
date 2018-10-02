using KinectX.Data;
using KinectX.IO;
using KinectX.Meta;
using KinectX.Registration;
using Microsoft.Kinect;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;

namespace KinectX.Network
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    [ServiceContract]
    public class KxServer
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private static CancellationTokenSource _cancallationTokenSrc = new CancellationTokenSource();

        private ushort[] depthShortBuffer = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
        private byte[] yuvByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 2];
        private byte[] rgbByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];
        private AutoResetEvent depthFrameReady = new AutoResetEvent(false);
        private AutoResetEvent yuvFrameReady = new AutoResetEvent(false);
        private AutoResetEvent rgbFrameReady = new AutoResetEvent(false);
        private AutoResetEvent jpegFrameReady = new AutoResetEvent(false);
        private AutoResetEvent audioFrameReady = new AutoResetEvent(false);
        private Queue<byte[]> audioFrameQueue = new Queue<byte[]>();
        private string _lastFilePath;

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
        public int GetArucoMarkerCount()
        {
            var yu2 = LatestYUVImage();
            var colorCv = new CvColor(yu2);
            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            return markers.Count;
        }

        [OperationContract]
        public byte[] FusionCameraPose()
        {
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Cube();
            var yu2 = LatestYUVImage();
            var colorCv = new CvColor(yu2);
            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);

            if (!markers.Any()) { return PoseFormatter.PoseToBytes(new double[4, 4]); }//zeros

            //Calculate pose
            var depth = LatestDepthImage();
            CameraSpacePoint[] _3dImage = new CameraSpacePoint[KinectSettings.COLOR_PIXEL_COUNT];
            KxBuffer.instance.coordinateMapper.MapColorFrameToCameraSpace(depth, _3dImage);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            var pose = kxTransform.FusionCameraPose;
            return PoseFormatter.PoseToBytes(pose);
        }

        [OperationContract]
        public byte[] CameraPose()
        {
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Cube();
            var yu2 = LatestYUVImage();
            var colorCv = new CvColor(yu2);
            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);

            if (!markers.Any()) { return PoseFormatter.PoseToBytes(new double[4, 4]); }//zeros

            //Calculate pose
            var depth = LatestDepthImage();
            CameraSpacePoint[] _3dImage = new CameraSpacePoint[KinectSettings.COLOR_PIXEL_COUNT];
            KxBuffer.instance.coordinateMapper.MapColorFrameToCameraSpace(depth, _3dImage);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            var pose = kxTransform.CameraPose;
            return PoseFormatter.PoseToBytes(pose);
        }

        [OperationContract]
        public bool RecordXef(TimeSpan duration)
        {
            try
            {
                _logger.Log(LogLevel.Info, $"Requested xef recording - {duration.TotalMilliseconds} ms.");
                var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _lastFilePath = Path.Combine(current, "record.xef");
                if (File.Exists(_lastFilePath))
                    File.Delete(_lastFilePath);
                Xef.Record(_lastFilePath, duration);
                Thread.Sleep((int)duration.Add(TimeSpan.FromSeconds(2)).TotalMilliseconds);
                _logger.Log(LogLevel.Info, $"Recording successful. Sending bytes from xef recording - {_lastFilePath}");
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.ToString());
                return false;
            }
        }

        [OperationContract]
        public byte[] LastRecording()
        {
            if (_lastFilePath == null || !File.Exists(_lastFilePath)) { return new byte[0]; }
            return File.ReadAllBytes(_lastFilePath);
        }

        [OperationContract]
        public byte[] LatestJPEGImage()
        {
            return new byte[0];
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
            _logger.Info("YUV frame requested...");
            this.yuvFrameReady.WaitOne();
            _logger.Info("YUV frame ready. Copying frame...");
            lock (KxBuffer.instance.yuvFrameReady)
                Buffer.BlockCopy((Array)KxBuffer.instance.yuvByteBuffer, 0, (Array)this.yuvByteBuffer, 0, KinectSettings.COLOR_PIXEL_COUNT * 2);
            _logger.Info("Returning frame");
            return this.yuvByteBuffer;
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

        public async static void Start()
        {
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    _logger.Info("Starting Kinect service...");
                    KxBuffer.StartBuffer();
                    ServiceHost serviceHost = new ServiceHost(typeof(KxServer));
                    serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                    serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
                    serviceHost.Open();
                    _logger.Info("Kinect listener service!");
                    Console.ReadLine();
                }, KxServer._cancallationTokenSrc.Token);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                Stop();
            }
        }

        public static void Stop()
        {
            try
            {
                _logger.Info("Stopping Kinect service...");
                KxServer._cancallationTokenSrc.Cancel();
                //KxBuffer.instance.Stop();
                //ServiceHost serviceHost = new ServiceHost(typeof(KxServer));
                //serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                //serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
                //serviceHost.Close();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                Stop();
            }
        }
    }
}
