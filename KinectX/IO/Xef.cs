using KinectX.Processors;
using KinectX.Meta;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using NLog;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static KinectX.Processors.DepthProcessor;

namespace KinectX.Help
{
    public class Xef
    {
        private string _path;
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private ushort[] _pixels;

        public Xef(string path)
        {
            _path = path;
        }

        public ColorProcessor LoadColorFrame(int desiredFrameNum, bool includeAlpha = false)
        {
            var cProcessor = new ColorProcessor();
            cProcessor.FrameSelector = new FrameSelector(desiredFrameNum, 1);

            using (KStudioClient client = KStudio.CreateClient(KStudioClientFlags.ProcessNotifications))
            {
                ManualResetEvent mr = new ManualResetEvent(false);
                KStudioEventStreamSelectorCollection selEvents = new KStudioEventStreamSelectorCollection();
                selEvents.Add(KStudioEventStreamDataTypeIds.UncompressedColor);

                //This is where you will intercept steps in the XEF file
                KStudioEventReader reader = client.CreateEventReader(_path, selEvents);

                KStudioEvent ev;
                while ((ev = reader.GetNextEvent()) != null && !cProcessor.FrameSelector.AreAllFramesCaptured)
                {
                    cProcessor.ProcessEvent(ev);
                }
                reader.Dispose();
            }
            return cProcessor;
        }

        public CoordinateMapper GetEmbeddedCoordinateMapper()
        {
            CoordinateMapper cm;
            using (var client = KStudio.CreateClient())
            {
                client.ConnectToService();
                using (var playBack = client.CreatePlayback(_path))
                {
                    var sensor = KinectSensor.GetDefault();
                    sensor.Open();
                    playBack.Start();
                    //Must let it play to capture the coordinate mapper
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(250);
                    }
                    cm = KinectSensor.GetDefault().CoordinateMapper;
                    playBack.Stop();
                }
            }
            return cm;
        }

        public CameraSpacePoint[] LoadCameraSpace(int frame, int numberofSmoothingFrames = 1)
        {
            CameraSpacePoint[] points = new CameraSpacePoint[KinectSettings.COLOR_HEIGHT * KinectSettings.COLOR_WIDTH];
            var cm = GetEmbeddedCoordinateMapper();
            var depth = LoadDepthFrame(frame, numberofSmoothingFrames)
                          .GetAverageFrame();
            cm.MapColorFrameToCameraSpace(depth, points);
            return points;
        }

        public ushort[] LoadDepthFrameFromPlayback(int desiredFrameNum)
        {
            var pixels = new ushort[KinectSettings.DEPTH_HEIGHT * KinectSettings.DEPTH_WIDTH];
            using (var client = KStudio.CreateClient())
            {
                client.ConnectToService();
                using (var playBack = client.CreatePlayback(_path))
                {
                    var sensor = KinectSensor.GetDefault();
                    sensor.Open();

                    var reader = sensor.DepthFrameSource.OpenReader();
                    reader.FrameArrived += Reader_FrameArrived;
                    playBack.Start();
                    while (playBack.State == KStudioPlaybackState.Playing)
                    {
                        Thread.Sleep(250);
                    }
                    //var depthTest = LoadDepthFrame(desiredFrameNum, 1).GetAverageFrame();
                    //CameraSpacePoint[] points = new CameraSpacePoint[KinectSettings.COLOR_HEIGHT * KinectSettings.COLOR_WIDTH];
                    //var cm = KinectSensor.GetDefault().CoordinateMapper;
                    //cm.MapColorFrameToCameraSpace(depthTest, points);
                }
            }
            return _pixels;
        }

        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            _pixels = new ushort[KinectSettings.DEPTH_HEIGHT * KinectSettings.DEPTH_WIDTH];
            var frame = e.FrameReference.AcquireFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_pixels);
            }
        }

        public DepthProcessor LoadDepthFrame(int desiredFrameNum, int numberOfDepthSmooths)
        {
            var dProcessor = new DepthProcessor();
            dProcessor.FrameSelector = new FrameSelector(desiredFrameNum, numberOfDepthSmooths);

            using (KStudioClient client = KStudio.CreateClient(KStudioClientFlags.ProcessNotifications))
            {
                ManualResetEvent mr = new ManualResetEvent(false);
                KStudioEventStreamSelectorCollection selEvents = new KStudioEventStreamSelectorCollection();
                selEvents.Add(KStudioEventStreamDataTypeIds.Depth);

                //This is where you will intercept steps in the XEF file
                KStudioEventReader reader = client.CreateEventReader(_path, selEvents);

                KStudioEvent ev;
                while ((ev = reader.GetNextEvent()) != null && !dProcessor.FrameSelector.AreAllFramesCaptured)
                {
                    dProcessor.ProcessEvent(ev);
                }
                reader.Dispose();
            }
            return dProcessor;
        }
    }
}
