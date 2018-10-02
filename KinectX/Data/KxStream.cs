using KinectX.IO;
using KinectX.Meta;
using KinectX.Registration;
using Microsoft.Kinect;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectX.Data
{
    public class KxStream : IDisposable
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

        public KxStream()
        {
            KxBuffer.StartBuffer();

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
        }

        public void Dispose()
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
            KxBuffer.instance.Stop();
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

        public byte[] LatestRGBImage()
        {
            this.rgbFrameReady.WaitOne();
            lock (KxBuffer.instance.rgbByteBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.rgbByteBuffer, 0, (Array)this.rgbByteBuffer, 0, KinectSettings.COLOR_PIXEL_COUNT * 4);
            return this.rgbByteBuffer;
        }

        public bool RecordXef(TimeSpan duration, string path)
        {
            try
            {
                _logger.Log(LogLevel.Info, $"Requested xef recording - {duration.TotalMilliseconds} ms.");
                if (File.Exists(path))
                    File.Delete(path);
                Xef.Record(path, duration);
                Thread.Sleep((int)duration.Add(TimeSpan.FromSeconds(2)).TotalMilliseconds);
                _logger.Log(LogLevel.Info, $"Recording successful. Bytes save to - {path}");
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.ToString());
                return false;
            }
        }

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

        public int GetArucoMarkerCount()
        {
            var yu2 = LatestYUVImage();
            var colorCv = new CvColor(yu2);
            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            return markers.Count;
        }

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

        public float LastColorGain()
        {
            return KxBuffer.instance.lastColorGain;
        }

        public long LastColorExposureTimeTicks()
        {
            return KxBuffer.instance.lastColorExposureTimeTicks;
        }

    }
}
