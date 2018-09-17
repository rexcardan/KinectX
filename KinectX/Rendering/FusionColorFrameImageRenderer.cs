using Microsoft.Kinect.Fusion;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public class FusionColorFrameImageRenderer : ImageRenderer<FusionColorImageFrame>
    {
        private int[] colorPixels;

        public FusionColorFrameImageRenderer(Image im) : base(im)
        {
        }

        protected unsafe override void RenderData(FusionColorImageFrame colorFrame)
        {
            if ((null == _image) || null == colorFrame)
            {
                return;
            }

            if (null == colorPixels || colorFrame.PixelDataLength != colorPixels.Length)
            {
                // Create pixel array of correct format
                colorPixels = new int[colorFrame.PixelDataLength];
            }

            if (null == bitmap || colorFrame.Width != bitmap.Width || colorFrame.Height != bitmap.Height)
            {
                // Create bitmap of correct format
                bitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set bitmap as source to UI image object
                if (_image != null)
                    _image.Source = bitmap;
            }

            // Copy pixel data to pixel buffer
            colorFrame.CopyPixelDataTo(colorPixels);

            // Write pixels to bitmap
            bitmap.WritePixels(
                    new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                    colorPixels,
                    bitmap.PixelWidth * sizeof(int),
                    0);
        }
    }
}
