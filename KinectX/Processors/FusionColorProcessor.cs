using KinectX.Meta;
using Microsoft.Kinect;
using Microsoft.Kinect.Fusion;
using System;
using System.Threading.Tasks;


namespace KinectX.Processors
{
    public class FusionColorProcessor
    {
        /// <summary>
        /// Threshold used in the visibility depth test to check if this depth value occlude an
        ///  object or not.
        /// </summary>
        private const ushort DepthVisibilityTestThreshold = 50; // 50mm

        /// <summary>
        /// Lock object for raw pixel access
        /// </summary>
        private object rawDataLock = new object();

        /// <summary>
        /// Store the min depth value in color space. Used to prune occlusion.
        /// </summary>
        private ushort[] depthVisibilityTestMap;

        /// <summary>
        /// Downsample factor of the color frame used by the depth visibility test.
        /// </summary>
        private const int ColorDownsampleFactor = 2;

        /// <summary>
        /// Mapping of depth pixels into color image
        /// </summary>
        private ColorSpacePoint[] colorCoordinates;

        /// <summary>
        /// The object we will constantly fill up in the MapColorToDepth method. We will just keep one copy so we don't have to keep allocating -RC
        /// </summary>
        public FusionColorImageFrame ResampledColorFrameDepthAligned { get; set; }

        // Allocate color points re-sampled to depth size mapped into depth frame of reference
        int[] resampledColorImagePixelsAlignedToDepth;

        /// <summary>
        /// Local copies of these values
        /// </summary>
        private int depthVisibilityTestMapWidth;
        private int depthVisibilityTestMapHeight;
        private bool mirrorDepth;

        public FusionColorProcessor()
        {
            this.depthVisibilityTestMapWidth = KinectSettings.COLOR_WIDTH / ColorDownsampleFactor;
            this.depthVisibilityTestMapHeight = KinectSettings.COLOR_HEIGHT / ColorDownsampleFactor;
            this.depthVisibilityTestMap = new ushort[this.depthVisibilityTestMapWidth * this.depthVisibilityTestMapHeight];

            // Allocate the depth-color mapping points
            this.colorCoordinates = new ColorSpacePoint[KinectSettings.DEPTH_PIXEL_COUNT];
            ResampledColorFrameDepthAligned = new FusionColorImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
            resampledColorImagePixelsAlignedToDepth = new int[KinectSettings.DEPTH_PIXEL_COUNT];
        }

        /// <summary>
        /// Process the color and depth inputs, converting the color into the depth space
        /// </summary>
        public unsafe FusionColorImageFrame MapColorToDepth(ushort[] depthImagePixels, byte[] colorImagePixels, CoordinateMapper cm, bool mirrorDepth = true)
        {
            cm.MapDepthFrameToColorSpace(depthImagePixels, this.colorCoordinates);

            this.mirrorDepth = mirrorDepth;

            lock (this.rawDataLock)
            {
                // Fill in the visibility depth map.
                Array.Clear(this.depthVisibilityTestMap, 0, this.depthVisibilityTestMap.Length);
                fixed (ushort* ptrDepthVisibilityPixels = this.depthVisibilityTestMap, ptrDepthPixels = depthImagePixels)
                {
                    for (int index = 0; index < depthImagePixels.Length; ++index)
                    {
                        if (!float.IsInfinity(this.colorCoordinates[index].X) && !float.IsInfinity(this.colorCoordinates[index].Y))
                        {
                            int x = (int)(System.Math.Floor(this.colorCoordinates[index].X + 0.5f) / ColorDownsampleFactor);
                            int y = (int)(System.Math.Floor(this.colorCoordinates[index].Y + 0.5f) / ColorDownsampleFactor);

                            if ((x >= 0) && (x < this.depthVisibilityTestMapWidth) &&
                                (y >= 0) && (y < this.depthVisibilityTestMapHeight))
                            {
                                int depthVisibilityTestIndex = (y * this.depthVisibilityTestMapWidth) + x;
                                if ((ptrDepthVisibilityPixels[depthVisibilityTestIndex] == 0) ||
                                    (ptrDepthVisibilityPixels[depthVisibilityTestIndex] > ptrDepthPixels[index]))
                                {
                                    ptrDepthVisibilityPixels[depthVisibilityTestIndex] = ptrDepthPixels[index];
                                }
                            }
                        }
                    }
                }

                if (this.mirrorDepth)
                {
                    // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
                    // not need access to the individual rgba components.
                    fixed (byte* ptrColorPixels = colorImagePixels)
                    {
                        int* rawColorPixels = (int*)ptrColorPixels;

                        Parallel.For(
                            0,
                            KinectSettings.DEPTH_HEIGHT,
                            y =>
                            {
                                int destIndex = y * KinectSettings.DEPTH_WIDTH;

                                for (int x = 0; x < KinectSettings.DEPTH_WIDTH; ++x, ++destIndex)
                                {
                                    // calculate index into depth array
                                    int colorInDepthX = (int)System.Math.Floor(colorCoordinates[destIndex].X + 0.5);
                                    int colorInDepthY = (int)System.Math.Floor(colorCoordinates[destIndex].Y + 0.5);
                                    int depthVisibilityTestX = (int)(colorInDepthX / ColorDownsampleFactor);
                                    int depthVisibilityTestY = (int)(colorInDepthY / ColorDownsampleFactor);
                                    int depthVisibilityTestIndex = (depthVisibilityTestY * this.depthVisibilityTestMapWidth) + depthVisibilityTestX;

                                    // make sure the depth pixel maps to a valid point in color space
                                    if (colorInDepthX >= 0 && colorInDepthX < KinectSettings.COLOR_WIDTH && colorInDepthY >= 0
                                        && colorInDepthY < KinectSettings.COLOR_HEIGHT && depthImagePixels[destIndex] != 0)
                                    {
                                        ushort depthTestValue = this.depthVisibilityTestMap[depthVisibilityTestIndex];

                                        if ((depthImagePixels[destIndex] - depthTestValue) < DepthVisibilityTestThreshold)
                                        {
                                            // Calculate index into color array
                                            int sourceColorIndex = colorInDepthX + (colorInDepthY * KinectSettings.COLOR_WIDTH);

                                            // Copy color pixel
                                            this.resampledColorImagePixelsAlignedToDepth[destIndex] = rawColorPixels[sourceColorIndex];
                                        }
                                        else
                                        {
                                            this.resampledColorImagePixelsAlignedToDepth[destIndex] = 0;
                                        }
                                    }
                                    else
                                    {
                                        this.resampledColorImagePixelsAlignedToDepth[destIndex] = 0;
                                    }
                                }
                            });
                    }
                }
                else
                {
                    // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
                    // not need access to the individual rgba components.
                    fixed (byte* ptrColorPixels = colorImagePixels)
                    {
                        int* rawColorPixels = (int*)ptrColorPixels;

                        // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                        // to give a viewpoint as though from behind the Kinect looking forward by default.
                        Parallel.For(
                            0,
                            KinectSettings.DEPTH_HEIGHT,
                            y =>
                            {
                                int destIndex = y * KinectSettings.DEPTH_WIDTH;
                                int flippedDestIndex = destIndex + (KinectSettings.DEPTH_WIDTH - 1); // horizontally mirrored

                                for (int x = 0; x < KinectSettings.DEPTH_WIDTH; ++x, ++destIndex, --flippedDestIndex)
                                {
                                    // calculate index into depth array
                                    int colorInDepthX = (int)System.Math.Floor(colorCoordinates[destIndex].X + 0.5);
                                    int colorInDepthY = (int)System.Math.Floor(colorCoordinates[destIndex].Y + 0.5);
                                    int depthVisibilityTestX = (int)(colorInDepthX / ColorDownsampleFactor);
                                    int depthVisibilityTestY = (int)(colorInDepthY / ColorDownsampleFactor);
                                    int depthVisibilityTestIndex = (depthVisibilityTestY * this.depthVisibilityTestMapWidth) + depthVisibilityTestX;

                                    // make sure the depth pixel maps to a valid point in color space
                                    if (colorInDepthX >= 0 && colorInDepthX < KinectSettings.COLOR_WIDTH && colorInDepthY >= 0
                                        && colorInDepthY < KinectSettings.COLOR_HEIGHT && depthImagePixels[destIndex] != 0)
                                    {
                                        ushort depthTestValue = this.depthVisibilityTestMap[depthVisibilityTestIndex];

                                        if ((depthImagePixels[destIndex] - depthTestValue) < DepthVisibilityTestThreshold)
                                        {
                                            // Calculate index into color array- this will perform a horizontal flip as well
                                            int sourceColorIndex = colorInDepthX + (colorInDepthY * KinectSettings.COLOR_WIDTH);

                                            // Copy color pixel
                                            this.resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = rawColorPixels[sourceColorIndex];
                                        }
                                        else
                                        {
                                            this.resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = 0;
                                        }
                                    }
                                    else
                                    {
                                        this.resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = 0;
                                    }
                                }
                            });
                    }
                }
            }
            ResampledColorFrameDepthAligned.CopyPixelDataFrom(this.resampledColorImagePixelsAlignedToDepth);
            return ResampledColorFrameDepthAligned;
        }

        public void DisposeResources()
        {
            if (null != this.ResampledColorFrameDepthAligned)
            {
                this.ResampledColorFrameDepthAligned.Dispose();
            }
        }

        /// <summary>
        /// Reset the mapped color image on reset or re-create of volume
        /// </summary>
        public void ResetColorImage()
        {
            if (null != this.ResampledColorFrameDepthAligned && null != this.resampledColorImagePixelsAlignedToDepth)
            {
                // Clear the mapped color image
                Array.Clear(this.resampledColorImagePixelsAlignedToDepth, 0, this.resampledColorImagePixelsAlignedToDepth.Length);
                this.ResampledColorFrameDepthAligned.CopyPixelDataFrom(this.resampledColorImagePixelsAlignedToDepth);
            }
        }
    }
}
