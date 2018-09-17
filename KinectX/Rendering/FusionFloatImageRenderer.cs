using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public class FusionFloatImageRenderer : ImageRenderer<FusionFloatImageFrame>
    {
        public FusionFloatImageRenderer(Image im) : base(im)
        {
        }

        protected override void RenderData(FusionFloatImageFrame depthFloatFrame)
        {
            float[] depthFloatFrameDepthPixels = new float[KinectSettings.DEPTH_PIXEL_COUNT];
            int[] depthFloatFramePixelsArgb = new int[KinectSettings.DEPTH_PIXEL_COUNT];
            if (null == depthFloatFrame)
            {
                return;
            }

            if (null == bitmap || depthFloatFrame.Width != bitmap.Width || depthFloatFrame.Height != bitmap.Height)
            {
                // Create bitmap of correct format
                bitmap = new WriteableBitmap(depthFloatFrame.Width, depthFloatFrame.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set bitmap as source to UI image object
                _image.Source = bitmap;
            }

            depthFloatFrame.CopyPixelDataTo(depthFloatFrameDepthPixels);

            // Calculate color of pixels based on depth of each pixel
            float range = 4.0f;
            float oneOverRange = (1.0f / range) * 256.0f;
            float minRange = 0.0f;

            Parallel.For(
            0,
            depthFloatFrame.Height,
            y =>
            {
                int index = y * depthFloatFrame.Width;
                for (int x = 0; x < depthFloatFrame.Width; ++x, ++index)
                {
                    float depth = depthFloatFrameDepthPixels[index];
                    int intensity = (depth >= minRange) ? ((byte)((depth - minRange) * oneOverRange)) : 0;

                    depthFloatFramePixelsArgb[index] = (255 << 24) | (intensity << 16) | (intensity << 8) | intensity; // set blue, green, red
                }
            });

            // Copy colored pixels to bitmap
            bitmap.WritePixels(
                        new Int32Rect(0, 0, depthFloatFrame.Width, depthFloatFrame.Height),
                        depthFloatFramePixelsArgb,
                        bitmap.PixelWidth * sizeof(int),
                        0);
        }
    }
}
