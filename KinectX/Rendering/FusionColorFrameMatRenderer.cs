using KinectX.Extensions;
using KinectX.Rendering.Interfaces;
using Microsoft.Kinect.Fusion;
using OpenCvSharp;

namespace KinectX.Rendering
{
    public class FusionColorFrameMatRenderer : IRenderer<FusionColorImageFrame>
    {
        private int[] colorPixels;
        Mat _mat;

        public void Render(FusionColorImageFrame colorFrame)
        {
            if (null == colorFrame)
            {
                return;
            }

            if (null == colorPixels || colorFrame.PixelDataLength != colorPixels.Length)
            {
                // Create pixel array of correct format
                colorPixels = new int[colorFrame.PixelDataLength];
            }

            if (null == _mat || colorFrame.Width != _mat.Width || colorFrame.Height != _mat.Height)
            {
                // Create bitmap of correct format
                _mat = new Mat(colorFrame.Height, colorFrame.Width, MatType.CV_8UC4, colorPixels);
            }

            // Copy pixel data to pixel buffer
            colorFrame.CopyPixelDataTo(colorPixels);
            _mat.ShowNoWait();
        }
    }
}
