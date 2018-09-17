using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using System;
using System.Threading.Tasks;

namespace KinectX.Fusion.Components
{
    public class DeltaCalculator
    {
        private Engine engine;

        /// <summary>
        /// Kinect color delta from reference frame data from AlignPointClouds
        /// </summary>
        public FusionColorImageFrame DownsampledDeltaFromReferenceFrameColorFrame { get; private set; }

        /// <summary>
        /// Per-pixel alignment values
        /// </summary>
        public FusionFloatImageFrame DeltaFromReferenceFrame { get; set; }

        /// <summary>
        /// Pixel buffer of delta from reference frame in 32bit color
        /// </summary>
        public int[] DeltaFromReferenceFramePixelsArgb { get; private set; }
        public int[] DownsampledDeltaFromReferenceColorPixels { get; private set; }
        public float[] DeltaFromReferenceFrameFloatPixels { get; private set; }

        public DeltaCalculator(Engine e)
        {
            this.engine = e;
            // Create float pixel array
            this.DeltaFromReferenceFrameFloatPixels = new float[KinectSettings.DEPTH_PIXEL_COUNT];

            // Create colored pixel array of correct format
            DeltaFromReferenceFramePixelsArgb = new int[KinectSettings.DEPTH_PIXEL_COUNT];

            //Downsampled Storage
            DownsampledDeltaFromReferenceFrameColorFrame = new FusionColorImageFrame(e.Resampler.DownsampledWidth, e.Resampler.DownsampledHeight);
            DownsampledDeltaFromReferenceColorPixels = new int[e.Resampler.DownsampledWidth * e.Resampler.DownsampledHeight];
        }
        /// <summary>
        /// Up sample color delta from reference frame with nearest neighbor - replicates pixels
        /// </summary>
        /// <param name="factor">The up sample factor (2=x*2,y*2, 4=x*4,y*4, 8=x*8,y*8, 16=x*16,y*16).</param>
        public unsafe void UpsampleColorDeltasFrameNearestNeighbor()
        {
            var resamplerRef = engine.Resampler;
            if (null == DownsampledDeltaFromReferenceFrameColorFrame || null == DownsampledDeltaFromReferenceColorPixels || null == DeltaFromReferenceFramePixelsArgb)
            {
                throw new ArgumentException("inputs null");
            }

            //Make sure factor is valid
            engine.Resampler.CheckFactor();

            int upsampleWidth = resamplerRef.DownsampledWidth * resamplerRef.DownsampleFactor;
            int upsampleHeight = resamplerRef.DownsampledHeight * resamplerRef.DownsampleFactor;

            if (KinectSettings.DEPTH_WIDTH != upsampleWidth || KinectSettings.DEPTH_HEIGHT != upsampleHeight)
            {
                throw new ArgumentException("upsampled image size != depth size");
            }

            DownsampledDeltaFromReferenceFrameColorFrame.CopyPixelDataTo(DownsampledDeltaFromReferenceColorPixels);

            // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
            // not need access to the individual rgba components.
            fixed (int* rawColorPixelPtr = DownsampledDeltaFromReferenceColorPixels)
            {
                int* rawColorPixels = (int*)rawColorPixelPtr;

                // Note we run this only for the source image height pixels to sparsely fill the destination with rows
                Parallel.For(
                    0,
                    resamplerRef.DownsampledHeight,
                    y =>
                    {
                        int destIndex = y * upsampleWidth * resamplerRef.DownsampleFactor;
                        int sourceColorIndex = y * resamplerRef.DownsampledWidth;

                        for (int x = 0; x < resamplerRef.DownsampledWidth; ++x, ++sourceColorIndex)
                        {
                            int color = rawColorPixels[sourceColorIndex];

                            // Replicate pixels horizontally
                            for (int colFactorIndex = 0; colFactorIndex < resamplerRef.DownsampleFactor; ++colFactorIndex, ++destIndex)
                            {
                                // Replicate pixels vertically
                                for (int rowFactorIndex = 0; rowFactorIndex < resamplerRef.DownsampleFactor; ++rowFactorIndex)
                                {
                                    // Copy color pixel
                                    DeltaFromReferenceFramePixelsArgb[destIndex + (rowFactorIndex * upsampleWidth)] = color;
                                }
                            }
                        }
                    });
            }

            int sizeOfInt = sizeof(int);
            int rowByteSize = resamplerRef.DownsampledHeight * sizeOfInt;

            // Duplicate the remaining rows with memcpy
            for (int y = 0; y < resamplerRef.DownsampledHeight; ++y)
            {
                // iterate only for the smaller number of rows
                int srcRowIndex = upsampleWidth * resamplerRef.DownsampleFactor * y;

                // Duplicate lines
                for (int r = 1; r < resamplerRef.DownsampleFactor; ++r)
                {
                    int index = upsampleWidth * ((y * resamplerRef.DownsampleFactor) + r);

                    System.Buffer.BlockCopy(
                        DeltaFromReferenceFramePixelsArgb, srcRowIndex * sizeOfInt, DeltaFromReferenceFramePixelsArgb, index * sizeOfInt, rowByteSize);
                }
            }
        }

        /// <summary>
        /// Calculates delta frame and returns if tracking succeeded
        /// </summary>
        /// <returns></returns>
        public bool CalculateDeltaFrame(Matrix4 calculatedCameraPose)
        {
            var pc = engine.PointCloudCalculator;
            var trackingSucceeded = FusionDepthProcessor.AlignPointClouds(
                  engine.PointCloudCalculator.DownsampledRaycastPointCloudFrame,
                  engine.PointCloudCalculator.DownsampledDepthPointCloudFrame,
                  FusionDepthProcessor.DefaultAlignIterationCount,
                  DownsampledDeltaFromReferenceFrameColorFrame,
                  ref calculatedCameraPose);

            UpsampleColorDeltasFrameNearestNeighbor();
            OnColorDeltaPixelReady(this.DeltaFromReferenceFramePixelsArgb);

            return trackingSucceeded;
        }

        public event ColorDeltaPixelReadyHandler ColorDeltaReady;
        public delegate void ColorDeltaPixelReadyHandler(object sender, int[] pixels);
        protected virtual void OnColorDeltaPixelReady(int[] pixels)
        {
            if (ColorDeltaReady != null)
                ColorDeltaReady(this, pixels);
        }

        public void UpdateAlignDeltas()
        {
            if (engine.PoseFinder.AutoFindCameraPoseWhenLost)
            {
                engine.RenderController.UpdateAlignDeltas(DeltaFromReferenceFramePixelsArgb);
            }
            else
            {
                engine.RenderController.UpdateAlignDeltas(DeltaFromReferenceFrame);
            }
        }
    }
}
