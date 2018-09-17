using KinectX.Meta;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Data
{
    /// <summary>
    /// A frame listener for a connected Kinect (live data)
    /// </summary>
    public class LiveFrameListener : FrameListener
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public override void Initialize()
        {
            _logger.Info("Creating live data listener...");
            KxBuffer.StartBuffer();
            lock (KxBuffer.instance.depthFrameReady)
                KxBuffer.instance.depthFrameReady.Add(this.DepthReadyEvent);
            lock (KxBuffer.instance.rgbFrameReady)
                KxBuffer.instance.rgbFrameReady.Add(this.ColorReadyEvent);

            KxBuffer.instance.coordinateMapperReady.WaitOne();
            this.CoordinateMapper = KxBuffer.instance.coordinateMapper;
        }

        public override ushort[] GetDepthImagePixels()
        {
            this.DepthReadyEvent.WaitOne();
            lock (KxBuffer.instance.depthShortBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.depthShortBuffer, 0, (Array)this.depthImagePixels, 0, KinectSettings.DEPTH_PIXEL_COUNT * 2);
            return depthImagePixels;
        }

        public override byte[] GetColorImagePixels()
        {
            this.ColorReadyEvent.WaitOne();
            lock (KxBuffer.instance.rgbByteBuffer)
                Buffer.BlockCopy((Array)KxBuffer.instance.rgbByteBuffer, 0, (Array)this.colorImagePixels, 0, KinectSettings.COLOR_PIXEL_COUNT * 4);
            return this.colorImagePixels;
        }
    }
}
