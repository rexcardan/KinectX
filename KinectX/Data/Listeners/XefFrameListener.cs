using KinectX.IO;
using KinectX.Meta;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KinectX.Data.Listeners
{
    public class XefFrameListener : FrameListener
    {
        private Xef _xef;
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private int nextDepthFrameNum = 0;
        private int nextColorFrameNum = 0;

        public ushort[] depthShortBuffer = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
        public byte[] rgbByteBuffer = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];

        private Timer _readyTimer;

        public override byte[] GetColorImagePixels()
        {
            var tempColor = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];
            lock (rgbByteBuffer)
            {
                Array.Copy(rgbByteBuffer, tempColor, tempColor.Length);
            }
            ColorReadyEvent.Reset();
            Task.Run(()=>CopyNextColor());
            return tempColor;
        }

        public override ushort[] GetDepthImagePixels()
        {
            var tempDepth = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
            lock (depthShortBuffer)
            {
                Array.Copy(depthShortBuffer, tempDepth, tempDepth.Length);
            }
            Task.Run(() => CopyNextDepth());
            return tempDepth;
        }

        private void CopyNextDepth()
        {
            var frame = _xef.LoadDepthFrame(nextDepthFrameNum);
            if (nextDepthFrameNum < _xef.NumOfDepthFrames - 1)
            {
                nextDepthFrameNum++;
            }
            else { nextDepthFrameNum = 0; }
            lock (depthShortBuffer)
            {
                Array.Copy(frame, depthShortBuffer, frame.Length);
            }
            this.DepthReadyEvent.Set();
        }

        private void CopyNextColor()
        {
            var frame = _xef.LoadColorFrame(nextColorFrameNum);
            if (nextColorFrameNum < _xef.NumOfColorFrames - 1)
            {
                nextColorFrameNum++;
            }
            else { nextColorFrameNum = 0; }

            lock (rgbByteBuffer)
            {
                Array.Copy(frame, rgbByteBuffer, frame.Length);
            }
            ColorReadyEvent.Set();
        }

        public void SetXefFile(string xefFilePath)
        {
            _xef = new Xef(xefFilePath);
            this.CoordinateMapper = _xef.GetEmbeddedCoordinateMapper();
            CopyNextDepth();
            CopyNextColor();
        }

        public override void Initialize()
        {
            
        }
    }
}
