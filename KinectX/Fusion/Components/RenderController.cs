using KinectX.Rendering;
using KinectX.Rendering.Interfaces;
using Microsoft.Kinect.Fusion;
using OpenCvSharp;
using System;
using System.Windows.Controls;

namespace KinectX.Fusion.Components
{
    public class RenderController
    {
        /// <summary>
        /// Force a point cloud calculation and render at least every 100 milliseconds.
        /// </summary>
        public const int RenderIntervalMilliseconds = 100;

        /// <summary>
        /// Timer stamp of last raycast and render
        /// </summary>
        public DateTime LastRenderTimestamp { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether rendering is overdue 
        /// (i.e. time interval since last render > RenderIntervalMilliseconds)
        /// </summary>
        public bool IsRenderOverdue
        {
            get
            {
                return (DateTime.UtcNow - LastRenderTimestamp).TotalMilliseconds >= RenderIntervalMilliseconds;
            }
        }

        #region IMAGE RENDERERS
        private PixelRenderer deltaPixelRenderer;
        private FusionFloatImageRenderer deltaFrameRenderer;
        private IRenderer<FusionColorImageFrame> reconstructionRenderer;
        private BytePixelRenderer colorRenderer;
        private FusionFloatImageRenderer depthRenderer;
        private IRenderer<Mat> matRenderer;

        #endregion

        public void SetReconstructionImage(Image im)
        {
            reconstructionRenderer = new FusionColorFrameImageRenderer(im);
        }

        public void RenderReconstructionAsMat()
        {
            reconstructionRenderer = new FusionColorFrameMatRenderer();
        }

        public void RenderMatAsMat()
        {
            matRenderer = new MatRenderer();
        }

        public void SetMatImage(Image im)
        {
            matRenderer = new MatImageRenderer(im);
        }

        public void SetDeltaImage(Image im)
        {
            deltaPixelRenderer = new PixelRenderer(im);
            deltaFrameRenderer = new FusionFloatImageRenderer(im);
        }

        public void UpdateAlignDeltas(int[] deltaFromReferenceFramePixelsArgb)
        {
            if (deltaPixelRenderer != null)
                deltaPixelRenderer.Render(deltaFromReferenceFramePixelsArgb);
        }

        public void SetColorImage(Image colorImage)
        {
            colorRenderer = new BytePixelRenderer(colorImage);
        }

        public void SetDepthImage(Image depthImage)
        {
            depthRenderer = new FusionFloatImageRenderer(depthImage);
        }

        public void RenderColor(byte[] colorPixels)
        {
            if (colorRenderer != null)
                colorRenderer.Render(colorPixels);
        }

        public void RenderReconstruction(FusionColorImageFrame cf)
        {
            if (reconstructionRenderer != null)
            {
                reconstructionRenderer.Render(cf);
                LastRenderTimestamp = DateTime.UtcNow;
            }
        }

        public void RenderMat(Mat m)
        {
            if (matRenderer != null)
                matRenderer.Render(m);
        }

        public void RendorDepth(FusionFloatImageFrame df)
        {
            if (depthRenderer != null)
                depthRenderer.Render(df);
        }

        public void UpdateAlignDeltas(FusionFloatImageFrame ff)
        {
            if (deltaFrameRenderer != null)
                deltaFrameRenderer.Render(ff);
        }
    }
}
