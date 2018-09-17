using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using System;
using System.Threading.Tasks;

namespace KinectX.Fusion.Components
{
    public class Resampler
    {
        /// <summary>
        /// Pixel buffer of depth float frame with pixel data in float format, downsampled for AlignPointClouds
        /// </summary>
        private float[] downsampledDepthImagePixels;

        public int DownsampleFactor { get; private set; } = 2;
        public int DownsampledWidth { get; private set; }
        public int DownsampledHeight { get; private set; }
        public int DownSampledImageSize { get; private set; }

        public Resampler(Engine e)
        {
            DownsampledWidth = KinectSettings.DEPTH_WIDTH / DownsampleFactor;
            DownsampledHeight = KinectSettings.DEPTH_HEIGHT / DownsampleFactor;
            DownSampledImageSize = DownsampledWidth * DownsampledHeight;
            this.downsampledDepthImagePixels = new float[DownSampledImageSize];
        }

        public FusionFloatImageFrame GetDefaultFloatFrame()
        {
            var downsampledWidth = KinectSettings.DEPTH_WIDTH / DownsampleFactor;
            var downsampledHeight = KinectSettings.DEPTH_HEIGHT / DownsampleFactor;

            // Allocate downsampled image frames
            return new FusionFloatImageFrame(downsampledWidth, downsampledHeight);
        }
        /// <summary>
        /// Downsample depth pixels with nearest neighbor
        /// </summary>
        /// <param name="dest">The destination depth image.</param>
        /// <param name="factor">The downsample factor (2=x/2,y/2, 4=x/4,y/4, 8=x/8,y/8, 16=x/16,y/16).</param>
        public unsafe void DownsampleDepthFrameNearestNeighbor(FusionFloatImageFrame dest, ushort[] depthImagePixels, bool mirrorDepth = false)
        {
            var factor = DownsampleFactor;

            if (null == dest || null == this.downsampledDepthImagePixels)
            {
                throw new ArgumentException("inputs null");
            }

            if (false == (2 == factor || 4 == factor || 8 == factor || 16 == factor))
            {
                throw new ArgumentException("factor != 2, 4, 8 or 16");
            }

            int downsampleWidth = KinectSettings.DEPTH_WIDTH / factor;
            int downsampleHeight = KinectSettings.DEPTH_HEIGHT / factor;

            if (dest.Width != downsampleWidth || dest.Height != downsampleHeight)
            {
                throw new ArgumentException("dest != downsampled image size");
            }

            if (mirrorDepth)
            {
                fixed (ushort* rawDepthPixelPtr = depthImagePixels)
                {
                    ushort* rawDepthPixels = (ushort*)rawDepthPixelPtr;

                    Parallel.For(
                        0,
                        downsampleHeight,
                        y =>
                        {
                            int destIndex = y * downsampleWidth;
                            int sourceIndex = y * KinectSettings.DEPTH_WIDTH * factor;

                            for (int x = 0; x < downsampleWidth; ++x, ++destIndex, sourceIndex += factor)
                            {
                                // Copy depth value
                                this.downsampledDepthImagePixels[destIndex] = (float)rawDepthPixels[sourceIndex] * 0.001f;
                            }
                        });
                }
            }
            else
            {
                fixed (ushort* rawDepthPixelPtr = depthImagePixels)
                {
                    ushort* rawDepthPixels = (ushort*)rawDepthPixelPtr;

                    // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                    // to give a viewpoint as though from behind the Kinect looking forward by default.
                    Parallel.For(
                        0,
                        downsampleHeight,
                        y =>
                        {
                            int flippedDestIndex = (y * downsampleWidth) + (downsampleWidth - 1);
                            int sourceIndex = y * KinectSettings.DEPTH_WIDTH * factor;

                            for (int x = 0; x < downsampleWidth; ++x, --flippedDestIndex, sourceIndex += factor)
                            {
                                // Copy depth value
                                this.downsampledDepthImagePixels[flippedDestIndex] = (float)rawDepthPixels[sourceIndex] * 0.001f;
                            }
                        });
                }
            }

            dest.CopyPixelDataFrom(this.downsampledDepthImagePixels);
        }

        public void CheckFactor()
        {
            if (false == (2 == DownsampleFactor || 4 == DownsampleFactor || 8 == DownsampleFactor || 16 == DownsampleFactor))
            {
                throw new ArgumentException("factor != 2, 4, 8 or 16");
            }
        }
    }
}
