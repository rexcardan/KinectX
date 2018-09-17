using KinectX.Meta;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public class PixelRenderer : ImageRenderer<int[]>
    {
        public PixelRenderer(Image im) : base(im)
        {
        }

        protected override void RenderData(int[] dataToRender)
        {
            if (null == bitmap || KinectSettings.DEPTH_WIDTH != bitmap.Width || KinectSettings.DEPTH_HEIGHT != bitmap.Height)
            {
                // Create bitmap of correct format
                bitmap = new WriteableBitmap(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set bitmap as source to UI image object
                _image.Source = bitmap;
            }

            // Copy colored pixels to bitmap
            bitmap.WritePixels(
                        new Int32Rect(0, 0, KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT),
                        dataToRender,
                        bitmap.PixelWidth * sizeof(int),
                        0);
        }
    }
}
