using KinectX.Meta;
using OpenCvSharp;

namespace KinectX.Data
{
    public class CvDepth : Mat
    {
        public CvDepth(ushort[] pixels) : base(KinectSettings.DEPTH_HEIGHT, KinectSettings.DEPTH_WIDTH, MatType.CV_16SC1)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (ushort)(pixels[i]);
            }

            //Copy depth data
            unsafe
            {
                var destPtr = (ushort*)this.Data;
                for (int i = 0; i < pixels.Length; ++i)
                {
                    *destPtr++ = pixels[i];
                }
            }
        }
    }
}
