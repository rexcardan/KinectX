using KinectX.Extensions;
using KinectX.Rendering.Interfaces;
using OpenCvSharp;

namespace KinectX.Rendering
{
    public class MatRenderer : IRenderer<Mat>
    {
        public void Render(Mat dataToRender)
        {
            dataToRender.ShowNoWait();
        }
    }
}
