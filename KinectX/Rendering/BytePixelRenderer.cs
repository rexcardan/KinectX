using KinectX.Meta;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public class BytePixelRenderer : ImageRenderer<byte[]>
    {
        public BytePixelRenderer(Image im) : base(im)
        {
        }

        protected override void RenderData(byte[] dataToRender)
        {
            if (null == bitmap || KinectSettings.COLOR_WIDTH != bitmap.Width || KinectSettings.COLOR_HEIGHT != bitmap.Height)
            {
                // Create bitmap of correct format
                bitmap = new WriteableBitmap(KinectSettings.COLOR_WIDTH, KinectSettings.COLOR_HEIGHT, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set bitmap as source to UI image object
                _image.Source = bitmap;
            }

            // Copy colored pixels to bitmap
            bitmap.WritePixels(
                        new Int32Rect(0, 0, KinectSettings.COLOR_WIDTH, KinectSettings.COLOR_HEIGHT),
                        dataToRender,
                        bitmap.PixelWidth * sizeof(int),
                        0);
        }
    }
}
