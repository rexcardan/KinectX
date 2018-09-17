using KinectX.Rendering.Interfaces;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KinectX.Rendering
{
    public abstract class ImageRenderer<T> : IRenderer<T>
    {
        protected WriteableBitmap bitmap = null;
        protected Image _image;

        public ImageRenderer(Image im)
        {
            _image = im;
        }

        public void Render(T dataToRender)
        {
            _image.Dispatcher.InvokeAsync(new Action(() =>
            {
                RenderData(dataToRender);
            }));
        }
        protected abstract void RenderData(T dataToRender);
    }
}
