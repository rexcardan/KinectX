using KinectX.Help;
using KinectX.Meta;
using Microsoft.Kinect.Tools;
using NLog;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Processors
{
    public class ColorProcessor
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private byte[] _yuy2_image = new byte[KinectSettings.COLOR_WIDTH * 2 * KinectSettings.COLOR_HEIGHT];

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
            if (ev.EventStreamDataTypeId == KStudioEventStreamDataTypeIds.UncompressedColor)
            {
                _logger.Info($"Processing frame new frame...");
                this.FrameSelector.IncrementFrame();
                if (!FrameSelector.IsDesiredFrame)
                {
                    { _logger.Info($"Rejecting frame {FrameSelector.CurrentFrameNum}. Threshold is {FrameSelector.InitialFrame}"); }
                }

                //Stored internally as YUY2 format, need to convert to RGBA
                _logger.Info($"Updating last frame {FrameSelector.CurrentFrameNum}...");
                ev.CopyEventDataToArray(_yuy2_image, 0);

                //  UpdateFrame(pixels);
                _logger.Info($"Processing completed on {FrameSelector.CurrentFrameNum}...");
            }
        }


        public byte[] LastFrame
        {
            get
            {
                var rgbaBytes = new byte[KinectSettings.COLOR_HEIGHT * KinectSettings.RGBA_STRIDE];
                using (var yuy2 = new Mat(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC2, _yuy2_image))
                {
                    using (var rgba = new Mat())
                    {
                        Cv2.CvtColor(yuy2, rgba, ColorConversionCodes.YUV2RGBA_YUY2);
                        Marshal.Copy(rgba.Data, rgbaBytes, 0, rgbaBytes.Length);
                    }
                }
                return rgbaBytes;
            }
        }

        public Mat ToCVImage(bool includeAlpha = false)
        {
            var m = new Mat(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC4, LastFrame);

            if (!includeAlpha)
            {
                Cv2.CvtColor(m, m, ColorConversionCodes.BGRA2BGR);
            }
            return m;
        }
    }
}
