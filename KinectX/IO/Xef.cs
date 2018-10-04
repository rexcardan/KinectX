using KinectX.Data;
using KinectX.Meta;
using KinectX.Processors;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using Microsoft.Win32.TaskScheduler;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace KinectX.IO
{
    public class Xef
    {
        private string _path;
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private ushort[] _pixels;

        public Xef(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("File does not exist!");
            }
            _path = Path.GetFullPath(path);
            NumOfDepthFrames = GetNumOfDepthFrames();
            NumOfColorFrames = GetNumOfColorFrames();
        }

        public CvColor LoadCvColorFrame(int desiredFrameNum)
        {
            var color = this.LoadColorFrame(desiredFrameNum);
            return new CvColor(color);
        }

        public CvDepth LoadCvDepthFrame(int desiredFrameNum)
        {
            var depth = this.LoadDepthFrame(desiredFrameNum);
            return new CvDepth(depth);
        }

        public int NumOfDepthFrames { get; private set; }
        public int NumOfColorFrames { get; private set; }

        public CoordinateMapper GetEmbeddedCoordinateMapper()
        {
            _logger.Info("Loading coordinate mapper from XEF file...");
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
            _logger.Info("Coordinate mapper loaded from XEF file!");
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

        public int GetNumOfDepthFrames()
        {
            _logger.Info("Calculating number of depth frames...");
            return GetNumOfFrames(KStudioEventStreamDataTypeIds.Depth);
        }

        public int GetNumOfColorFrames()
        {
            _logger.Info("Calculating number of color frames...");
            return GetNumOfFrames(KStudioEventStreamDataTypeIds.UncompressedColor);
        }

        public int GetNumOfFrames(Guid typeId)
        {
            int numFrames = 0;
            try
            {
                using (KStudioClient client = KStudio.CreateClient(KStudioClientFlags.ProcessNotifications))
                {
                    ManualResetEvent mr = new ManualResetEvent(false);
                    KStudioEventStreamSelectorCollection selEvents = new KStudioEventStreamSelectorCollection();
                    selEvents.Add(typeId);
                    //This is where you will intercept steps in the XEF file
                    using (var reader = client.CreateEventReader(_path, selEvents))
                    {
                        KStudioEvent ev;
                        reader.GetNextEvent();
                        while ((ev = reader.GetNextEvent()) != null)
                        {
                            numFrames++;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if(e.Message == "Operation is not valid due to the current state of the object.")
                {
                    throw new Exception($"No events of type {typeId} in this XEF file!");
                }
            }
            return numFrames;
        }

        public T[] LoadFrame<T>(int desiredFrameNum, Guid eventType, Func<KStudioEvent, T[]> pixelCopyOperation)
        {
            T[] pixels = null;

            using (KStudioClient client = KStudio.CreateClient(KStudioClientFlags.ProcessNotifications))
            {
                KStudioEventStreamSelectorCollection selEvents = new KStudioEventStreamSelectorCollection();
                selEvents.Add(eventType);

                //This is where you will intercept steps in the XEF file
                using (var reader = client.CreateEventReader(_path, selEvents))
                {
                    KStudioEvent ev;
                    reader.GetNextEvent();
                    int frameNum = 0;
                    while ((ev = reader.GetNextEvent()) != null)
                    {
                        if (frameNum == desiredFrameNum)
                        {
                            pixels = pixelCopyOperation(ev);
                            break;
                        }
                        frameNum++;
                    }
                }
            }
            return pixels;
        }

        public unsafe ushort[] LoadDepthFrame(int desiredFrame)
        {
            _logger.Info($"Loading depth frame {desiredFrame}...");
            var copyOperation = new Func<KStudioEvent, ushort[]>(ev =>
            {
                ushort[] pixels = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
                fixed (ushort* p = pixels)
                {
                    IntPtr ptr = (IntPtr)p;
                    ev.CopyEventDataToBuffer((uint)pixels.Length * sizeof(ushort), ptr);
                }
                return pixels;
            });
            var frame = LoadFrame(desiredFrame, KStudioEventStreamDataTypeIds.Depth, copyOperation);
            _logger.Info($"Loaded depth frame {desiredFrame}!");
            return frame;
        }

        public unsafe byte[] LoadColorFrame(int desiredFrame)
        {
            _logger.Info($"Loading color frame {desiredFrame}...");
            var copyOperation = new Func<KStudioEvent, byte[]>(ev =>
            {
                byte[] pixels = new byte[KinectSettings.COLOR_PIXEL_COUNT*2];
                fixed (byte* p = pixels)
                {
                    IntPtr ptr = (IntPtr)p;
                    ev.CopyEventDataToBuffer((uint)pixels.Length * sizeof(int), ptr);
                }
                
                return pixels;
            });
            var frame = LoadFrame(desiredFrame, KStudioEventStreamDataTypeIds.UncompressedColor, copyOperation);
            _logger.Info($"Loaded color frame {desiredFrame}!");
            return frame;
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

        public static void Record(string path, TimeSpan duration)
        {
            var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _logger.Log(LogLevel.Info, $"Requested xef recording - {duration.TotalMilliseconds} ms.");
            var info = new ProcessStartInfo();
            current = Path.Combine(current, "KSUtil.exe");
            var args = $"-record {path} {duration.TotalSeconds} -stream depth ir color";
            var ts = TaskService.Instance;
            ts.Execute(current).WithArguments(args).Once().AsTask("KxRecordTask").Run();
            ts.RootFolder.DeleteTask("KxRecordTask",true);
        }
    }
}
