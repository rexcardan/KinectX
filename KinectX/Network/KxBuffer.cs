using KinectX.Meta;
using Microsoft.Kinect;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinectX.Network
{
    /// <summary>
    /// Stores the latest images and data from the connected Kinect
    /// </summary>
    public class KxBuffer
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public ushort[] depthShortBuffer = new ushort[KinectSettings.DepthPixelCount];
        public byte[] depthByteBuffer = new byte[KinectSettings.DepthPixelCount * 2];
        public List<AutoResetEvent> depthFrameReady = new List<AutoResetEvent>();
        public byte[] yuvByteBuffer = new byte[KinectSettings.ColorPixelCount * 2];
        public List<AutoResetEvent> yuvFrameReady = new List<AutoResetEvent>();
        public byte[] rgbByteBuffer = new byte[KinectSettings.ColorPixelCount * 4];
        public List<AutoResetEvent> rgbFrameReady = new List<AutoResetEvent>();
        public int nJpegBytes = 0;
        public ManualResetEvent kinect2CalibrationReady = new ManualResetEvent(false);
        private Stopwatch stopWatch = new Stopwatch();
        private Body[] bodies = (Body[])null;
        public List<AutoResetEvent> audioFrameReady = new List<AutoResetEvent>();
        public List<Queue<byte[]>> audioFrameQueues = new List<Queue<byte[]>>();
        public static KxBuffer instance;
        private KinectSensor kinectSensor;
        private DepthFrameReader depthFrameReader;
        private ColorFrameReader colorFrameReader;
        public float lastColorGain;
        public long lastColorExposureTimeTicks;
        private BodyFrameReader bodyFrameReader;
        private AudioBeamFrameReader audioBeamFrameReader;

        public KxBuffer()
        {
            KxBuffer.instance = this;
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.CoordinateMapper.CoordinateMappingChanged += CoordinateMapper_CoordinateMappingChanged;
            this.kinectSensor.Open();
            
            _logger.Info(string.Format("Sensor {0} open and streaming...", this.kinectSensor.UniqueKinectId));
        }

        private void CoordinateMapper_CoordinateMappingChanged(object sender, CoordinateMappingChangedEventArgs e)
        {
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            this.depthFrameReader.FrameArrived += DepthFrameReader_FrameArrived;
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            this.audioBeamFrameReader = this.kinectSensor.AudioSource.OpenReader();
            this.audioBeamFrameReader.FrameArrived += AudioBeamFrameReader_FrameArrived;
            this.audioBeamFrameReader.AudioSource.AudioBeams[0].AudioBeamMode = AudioBeamMode.Automatic;
            this.audioBeamFrameReader.AudioSource.AudioBeams[0].BeamAngle=0.0f;
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

                    if ((rgbFrameReady.Count > 0))
                    {
                        lock (rgbByteBuffer)
                            colorFrame.CopyConvertedFrameDataToArray(rgbByteBuffer, ColorImageFormat.Bgra);
                        lock (rgbFrameReady)
                            foreach (var autoResetEvent in rgbFrameReady)
                                autoResetEvent.Set();
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
                    if (depthFrameReady.Count > 0)
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
