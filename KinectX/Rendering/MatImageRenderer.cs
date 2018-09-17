using OpenCvSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public class MatImageRenderer : ImageRenderer<Mat>
    {
        public MatImageRenderer(Image im) : base(im)
        {
        }

        protected unsafe override void RenderData(Mat dataToRender)
        {
            var matWidth = dataToRender.Cols;
            var matHeight = dataToRender.Rows;

            if (null == bitmap || matWidth != bitmap.Width || matHeight != bitmap.Height)
            {
                PixelFormat format = PixelFormats.Bgr32;
                switch (dataToRender.ElemSize())
                {
                    case 2:
                        format = PixelFormats.BlackWhite;
                        break;
                    case 3:
                        format = PixelFormats.Bgr24;
                        break;
                    case 4:
                        format = PixelFormats.Bgr32;
                        break;
                    default:
                        format = PixelFormats.Bgr32;
                        break;
                }

                // Create bitmap of correct format
                bitmap = new WriteableBitmap(matWidth, matHeight, 96.0, 96.0, format, null);

                // Set bitmap as source to UI image object
                _image.Source = bitmap;
            }
            var stride = dataToRender.Cols * dataToRender.ElemSize();
            bitmap.WritePixels(
                   new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                   dataToRender.Data,
                   matWidth * matHeight * dataToRender.ElemSize(),
                   stride);
        }
    }
}
