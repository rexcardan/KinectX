using KinectX.Help;
using KinectX.Meta;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using NLog;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace KinectX.Processors
{
    public class DepthProcessor
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private long[] depthStore = new long[KinectSettings.DEPTH_HEIGHT * KinectSettings.DEPTH_WIDTH];
        private int numFrames = 0;

        public DepthProcessor()
        {
        }

        /// <summary>
        /// Adds a depth frame to the stack (for depth averaging)
        /// </summary>
        /// <param name="depthFrame"></param>
        public void AddDoseFrame(ushort[] depthFrame)
        {
            if (depthFrame.Length != (KinectSettings.DEPTH_WIDTH * KinectSettings.DEPTH_HEIGHT))
                throw new ArgumentException("Depth frame is not correct size");

            for (int i = 0; i < depthFrame.Length; i++)
            {
                depthStore[i] += depthFrame[i];
            }
            numFrames++;
        }

        /// <summary>
        /// Averages all frames which have been added through the AddDoseFrame method
        /// </summary>
        /// <returns>the average depth frame</returns>
        public ushort[] GetAverageFrame()
        {
            ushort[] depthAvg = new ushort[KinectSettings.DEPTH_HEIGHT * KinectSettings.DEPTH_WIDTH];
            for (int i = 0; i < depthAvg.Length; i++)
            {
                depthAvg[i] = (ushort)(Math.Round((double)depthStore[i] / numFrames));
            }
            return depthAvg;
        }

        /// <summary>
        /// Converts the depth array to a Mat image for viewing
        /// </summary>
        /// <param name="depthScale"></param>
        /// <returns></returns>
        public Mat ToCVImage(double viewScale = 1000)
        {
            var pixels = GetAverageFrame();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (ushort)(pixels[i] * viewScale);
            }
            return new Mat(KinectSettings.DEPTH_HEIGHT, KinectSettings.DEPTH_WIDTH, MatType.CV_16SC1, pixels);
        }

        /// <summary>
        /// A selector which allows discrimination of frames recieved
        /// </summary>
        public FrameSelector FrameSelector { get; set; }

        /// <summary>
        /// Processes and integrates a frame from an XEF file
        /// </summary>
        /// <param name="ev">the depth event recieved from the listener</param>
        public unsafe void ProcessEvent(KStudioEvent ev)
        {
            if (ev.EventStreamDataTypeId == KStudioEventStreamDataTypeIds.Depth)
            {
                _logger.Info($"Processing frame new frame...");
                this.FrameSelector.IncrementFrame();
                if (!FrameSelector.IsDesiredFrame)
                {
                    { _logger.Info($"Rejecting frame {FrameSelector.CurrentFrameNum}. Threshold is {FrameSelector.InitialFrame}"); }
                }
                else
                {
                    var pixels = new ushort[KinectSettings.DEPTH_HEIGHT * KinectSettings.DEPTH_WIDTH];
                    fixed (ushort* p = pixels)
                    {
                        IntPtr ptr = (IntPtr)p;
                        ev.CopyEventDataToBuffer((uint)pixels.Length * 2, ptr);
                    }
                    _logger.Info($"Integrating frame {FrameSelector.CurrentFrameNum}...");
                    AddDoseFrame(pixels);
                    _logger.Info($"Processing completed on {FrameSelector.CurrentFrameNum}...");
                }
            }
        }
    }
}
