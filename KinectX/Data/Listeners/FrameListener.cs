using KinectX.Meta;
using Microsoft.Kinect;
using System.Threading;

namespace KinectX.Data.Listeners
{
    public abstract class FrameListener
    {
        public ManualResetEvent ColorReadyEvent { get; } = new ManualResetEvent(false);
        public ManualResetEvent DepthReadyEvent { get; } = new ManualResetEvent(false);
       
        protected ushort[] depthImagePixels = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
        
        /// <summary>
        /// Intermediate storage for the extended depth data received from the camera in the current frame
        /// </summary>
        public abstract ushort[] GetDepthImagePixels();

        protected byte[] colorImagePixels = new byte[KinectSettings.RGBA_STRIDE * KinectSettings.COLOR_HEIGHT];

        /// <summary>
        /// Intermediate storage for the color data received from the camera in 32bit color
        /// </summary>
        public abstract byte[] GetColorImagePixels();

        public CoordinateMapper CoordinateMapper { get; set; }

        public abstract void Initialize();
    }
}
