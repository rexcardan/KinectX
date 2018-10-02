using KinectX.Meta;
using Microsoft.Kinect;
using NLog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace KinectX.Data
{
    /// <summary>
    /// Stores the latest images and data from the connected Kinect
    /// </summary>
    public class KxBuffer
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public static void StartBuffer()
        {
            if (KxBuffer.instance == null)
            {
                KxBuffer.instance = new KxBuffer();
            }
        }

        public ushort[] depthShortBuffer = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];

        public byte[] depthByteBuffer = new byte[KinectSettings.DEPTH_PIXEL_COUNT * 2];
        public List<EventWaitHandle> depthFrameReady = new List<EventWaitHandle>();
        public byte[] yuvByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 2];
        public List<EventWaitHandle> yuvFrameReady = new List<EventWaitHandle>();
        public byte[] rgbByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];
        public List<EventWaitHandle> rgbFrameReady = new List<EventWaitHandle>();
        public int nJpegBytes = 0;
        public ManualResetEvent coordinateMapperReady = new ManualResetEvent(false);
        private Stopwatch stopWatch = new Stopwatch();
        private Body[] bodies = (Body[])null;
        public List<EventWaitHandle> audioFrameReady = new List<EventWaitHandle>();
        public List<Queue<byte[]>> audioFrameQueues = new List<Queue<byte[]>>();
        public static KxBuffer instance;
        private KinectSensor kinectSensor;
        private DepthFrameReader depthFrameReader;
        private ColorFrameReader colorFrameReader;
        public float lastColorGain;
        public long lastColorExposureTimeTicks;
        private BodyFrameReader bodyFrameReader;
        private AudioBeamFrameReader audioBeamFrameReader;
        public CoordinateMapper coordinateMapper;

        private KxBuffer()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.CoordinateMapper.CoordinateMappingChanged += CoordinateMapper_CoordinateMappingChanged;
            this.kinectSensor.Open();

            _logger.Info(string.Format("Sensor {0} open and streaming...", this.kinectSensor.UniqueKinectId));
        }

        public void Stop()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.depthFrameReader.FrameArrived -= DepthFrameReader_FrameArrived;
            this.colorFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
            this.bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;
            this.audioBeamFrameReader.FrameArrived -= AudioBeamFrameReader_FrameArrived;
            this.kinectSensor.CoordinateMapper.CoordinateMappingChanged -= CoordinateMapper_CoordinateMappingChanged;
            this.kinectSensor.Close();
            KxBuffer.instance = null;
        }

        private void CoordinateMapper_CoordinateMappingChanged(object sender, CoordinateMappingChangedEventArgs e)
        {
            _logger.Info(string.Format("Coordinate mapper changed. Configuring events...", this.kinectSensor.UniqueKinectId));
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            this.depthFrameReader.FrameArrived += DepthFrameReader_FrameArrived;
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            this.audioBeamFrameReader = this.kinectSensor.AudioSource.OpenReader();
            this.audioBeamFrameReader.FrameArrived += AudioBeamFrameReader_FrameArrived;
            this.audioBeamFrameReader.AudioSource.AudioBeams[0].AudioBeamMode = AudioBeamMode.Automatic;
            this.audioBeamFrameReader.AudioSource.AudioBeams[0].BeamAngle = 0.0f;
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            coordinateMapperReady.Set();
        }

        private void AudioBeamFrameReader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
        {
            var audioBeamFrames = e.FrameReference.AcquireBeamFrames();
            if (audioBeamFrames != null)
            {
                var audioBeamFrame = audioBeamFrames[0];

                foreach (var subFrame in audioBeamFrame.SubFrames)
                {
                    var buffer = new byte[subFrame.FrameLengthInBytes];

                    subFrame.CopyFrameDataToArray(buffer);

                    lock (audioFrameQueues)
                    {
                        foreach (var queue in audioFrameQueues)
                        {
                            if (queue.Count > 10)
                                queue.Dequeue();
                            queue.Enqueue(buffer);
                        }
                    }

                    lock (audioFrameReady)
                        foreach (var autoResetEvent in audioFrameReady)
                            autoResetEvent.Set();
                    subFrame.Dispose();
                }

                audioBeamFrame.Dispose();
                audioBeamFrames.Dispose();

            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            var bodyFrame = e.FrameReference.AcquireFrame();
            if (bodyFrame != null)
            {
                using (bodyFrame)
                {
                    if (bodies == null)
                        bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);
                }
            }
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            var colorFrame = e.FrameReference.AcquireFrame();
            if (colorFrame != null)
            {
                using (colorFrame)
                {
                    lastColorGain = colorFrame.ColorCameraSettings.Gain;
                    lastColorExposureTimeTicks = colorFrame.ColorCameraSettings.ExposureTime.Ticks;

                    if (yuvFrameReady.Count > 0)
                    {
                        lock (yuvByteBuffer)
                            colorFrame.CopyRawFrameDataToArray(yuvByteBuffer);
                        lock (yuvFrameReady)
                            foreach (var autoResetEvent in yuvFrameReady)
                                autoResetEvent.Set();
                    }

                    if (rgbFrameReady.Any(ready => !ready.WaitOne(0)))
                    {
                        lock (rgbByteBuffer)
                            colorFrame.CopyConvertedFrameDataToArray(rgbByteBuffer, ColorImageFormat.Bgra);
                        lock (rgbFrameReady)
                            foreach (var autoResetEvent in rgbFrameReady)
                            {
                                autoResetEvent.Set();
                            }

                    }
                }
            }
        }

        private void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var depthFrame = e.FrameReference.AcquireFrame();
            if (depthFrame != null)
            {
                using (depthFrame)
                {
                    if (depthFrameReady.Any(ready => !ready.WaitOne(0)))
                    {
                        lock (depthShortBuffer)
                            depthFrame.CopyFrameDataToArray(depthShortBuffer);
                        lock (depthFrameReady)
                            foreach (var autoResetEvent in depthFrameReady)
                                autoResetEvent.Set();
                    }
                }
            }
        }
    }
}
