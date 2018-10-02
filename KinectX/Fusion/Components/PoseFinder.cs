using KinectX.Fusion.Helpers;
using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using NLog;
using System.Globalization;
using System.Threading.Tasks;

namespace KinectX.Fusion.Components
{
    public class PoseFinder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private FusionVolume volume;
        private CameraPoseFinder finder;

        /// <summary>
        /// Parameter to enable automatic finding of camera pose when lost. This searches back through
        /// the camera pose history where key-frames and camera poseCount have been stored in the camera
        /// pose finder database to propose the most likely pose matches for the current camera input.
        /// </summary>
        public bool AutoFindCameraPoseWhenLost { get; set; } = false;

        /// <summary>
        /// The maximum number of matched poseCount we consider when finding the camera pose. 
        /// Although the matches are ranked, so we look at the highest probability match first, a higher 
        /// value has a greater chance of finding a good match overall, but has the trade-off of being 
        /// slower. Typically we test up to around the 5 best matches, after which is may be better just
        /// to try again with the next input depth frame if no good match is found.
        /// </summary>
        private const int MaxCameraPoseFinderPoseTests = 5;

        /// <summary>
        /// CameraPoseFinderDistanceThresholdReject is a threshold used following the minimum distance 
        /// calculation between the input frame and the camera pose finder database. This calculated value
        /// between 0 and 1.0f must be less than or equal to the threshold in order to run the pose finder,
        /// as the input must at least be similar to the pose finder database for a correct pose to be
        /// matched.
        /// </summary>
        private const float CameraPoseFinderDistanceThresholdReject = 1.0f; // a value of 1.0 means no rejection

        /// <summary>
        /// The height of raw depth stream if keep the w/h ratio as 4:3
        /// </summary>
        private const int RawDepthHeightWithSpecialRatio = 384;

        /// <summary>
        /// Width of raw depth stream
        /// </summary>
        private const int RawDepthWidth = 512;

        /// <summary>
        /// Height of raw depth stream
        /// </summary>
        private const int RawDepthHeight = 424;

        public bool IsAvailable()
        {
            return AutoFindCameraPoseWhenLost
                && finder!=null
                && finder.GetStoredPoseCount() > 0;
        }

        /// <summary>
        /// Width of raw color stream
        /// </summary>
        private const int RawColorWidth = 1920;

        /// <summary>
        /// Height of raw color stream
        /// </summary>
        private const int RawColorHeight = 1080;

        /// <summary>
        /// Alignment energy from AlignDepthFloatToReconstruction for current frame 
        /// </summary>
        private float alignmentEnergy;

        /// <summary>
        /// Temp storage for resampled color
        /// </summary>
        private int[] resampledColorImagePixels;
        private Engine engine;

        public Matrix4 WorldToCameraTransform { get; private set; }

        public PoseFinder(Engine e)
        {
            this.volume = e.FusionVolume;
            this.engine = e;
            // Create a camera pose finder with default parameters
            CameraPoseFinderParameters cameraPoseFinderParams = CameraPoseFinderParameters.Defaults;
            finder = CameraPoseFinder.FusionCreateCameraPoseFinder(cameraPoseFinderParams);
            resampledColorImagePixels = new int[KinectSettings.DEPTH_PIXEL_COUNT];
        }

        /// <summary>
        /// Perform camera pose finding when tracking is lost using AlignPointClouds.
        /// This is typically more successful than FindCameraPoseAlignDepthFloatToReconstruction.
        /// </summary>
        /// <returns>Returns true if a valid camera pose was found, otherwise false.</returns>
        public bool FindCameraPoseAlignPointClouds(byte[] colorPixels, FusionFloatImageFrame depthFloatFrame)
        {
            if (finder == null)
            {
                return false;
            }

            var resampledColorFrame = ProcessColorForCameraPoseFinder(colorPixels);
            if (resampledColorFrame.Width == 0) { return false; } //Didn't work

            MatchCandidates matchCandidates = finder.FindCameraPose(
                depthFloatFrame,
                resampledColorFrame);

            if (null == matchCandidates)
            {
                return false;
            }

            int poseCount = matchCandidates.GetPoseCount();
            float minDistance = matchCandidates.CalculateMinimumDistance();

            if (0 == poseCount || minDistance >= CameraPoseFinderDistanceThresholdReject)
            {
                logger.Log(LogLevel.Warn, "Not enough matches");
                return false;
            }

            // Smooth the depth frame
            var smooth = volume.Engine.DepthProcessor.SmoothDepthFloatFrame(depthFloatFrame);

            engine.PointCloudCalculator.UpdateDepthPointCloud(smooth);
            // Calculate point cloud from the smoothed frame

            double smallestEnergy = double.MaxValue;
            int smallestEnergyNeighborIndex = -1;

            int bestNeighborIndex = -1;
            Matrix4 bestNeighborCameraPose = Matrix4.Identity;

            double bestNeighborAlignmentEnergy = Constants.MaxAlignPointCloudsEnergyForSuccess;

            // Run alignment with best matched poseCount (i.e. k nearest neighbors (kNN))
            int maxTests = System.Math.Min(MaxCameraPoseFinderPoseTests, poseCount);

            var neighbors = matchCandidates.GetMatchPoses();

            for (int n = 0; n < maxTests; n++)
            {
                // Run the camera tracking algorithm with the volume
                // this uses the raycast frame and pose to find a valid camera pose by matching the raycast against the input point cloud
                Matrix4 poseProposal = neighbors[n];

                // Get the saved pose view by raycasting the volume
                var pc = engine.PointCloudCalculator;
                this.volume.Reconstruction.CalculatePointCloud(pc.RaycastPointCloudFrame, poseProposal);

                bool success = this.volume.Reconstruction.AlignPointClouds(
                    pc.RaycastPointCloudFrame,
                    pc.DepthPointCloudFrame,
                    FusionDepthProcessor.DefaultAlignIterationCount,
                    resampledColorFrame,
                    out this.alignmentEnergy,
                    ref poseProposal);

                bool relocSuccess = success && this.alignmentEnergy < bestNeighborAlignmentEnergy && this.alignmentEnergy > Constants.MinAlignPointCloudsEnergyForSuccess;

                if (relocSuccess)
                {
                    bestNeighborAlignmentEnergy = this.alignmentEnergy;
                    bestNeighborIndex = n;

                    // This is after tracking succeeds, so should be a more accurate pose to store...
                    bestNeighborCameraPose = poseProposal;

                    // Update the delta image
                    resampledColorFrame.CopyPixelDataTo(engine.DeltaCalculator.DeltaFromReferenceFramePixelsArgb);
                }

                // Find smallest energy neighbor independent of tracking success
                if (this.alignmentEnergy < smallestEnergy)
                {
                    smallestEnergy = this.alignmentEnergy;
                    smallestEnergyNeighborIndex = n;
                }
            }

            matchCandidates.Dispose();

            // Use the neighbor with the smallest residual alignment energy
            // At the cost of additional processing we could also use kNN+Mean camera pose finding here
            // by calculating the mean pose of the best n matched poses and also testing this to see if the 
            // residual alignment energy is less than with kNN.
            if (bestNeighborIndex > -1)
            {
                this.WorldToCameraTransform = bestNeighborCameraPose;
                this.SetReferenceFrame(this.WorldToCameraTransform);

                // Tracking succeeded!
                engine.CameraTracker.SetTrackingSucceeded();

                engine.DeltaCalculator.UpdateAlignDeltas();

                logger.Log(LogLevel.Warn, "Camera Pose Finder SUCCESS! Residual energy= " + string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", bestNeighborAlignmentEnergy) + ", " + poseCount + " frames stored, minimum distance=" + minDistance + ", best match index=" + bestNeighborIndex);

                return true;
            }
            else
            {
                this.WorldToCameraTransform = neighbors[smallestEnergyNeighborIndex];
                this.SetReferenceFrame(this.WorldToCameraTransform);

                // Camera pose finding failed - return the tracking failed error code
                engine.CameraTracker.SetTrackingFailed();

                // Tracking Failed will be set again on the next iteration in ProcessDepth
                logger.Log(LogLevel.Warn, "Camera Pose Finder FAILED! Residual energy=" + string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", smallestEnergy) + ", " + poseCount + " frames stored, minimum distance=" + minDistance + ", best match index=" + smallestEnergyNeighborIndex);
                return false;
            }
        }

        /// <summary>
        /// Process input color image to make it equal in size to the depth image
        /// </summary>
        private unsafe FusionColorImageFrame ProcessColorForCameraPoseFinder(byte[] colorImagePixels)
        {
            var resampledColorFrame = new FusionColorImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);

            if (KinectSettings.DEPTH_WIDTH != RawDepthWidth || KinectSettings.DEPTH_HEIGHT != RawDepthHeight
                || KinectSettings.COLOR_WIDTH != RawColorWidth || KinectSettings.COLOR_HEIGHT != RawColorHeight)
            {
                logger.Log(LogLevel.Error, "Cannot perform ProcessColorForCameraPoseFinder. Dimensions don't agree.");
                return new FusionColorImageFrame(0, 0);
            }

            float factor = RawColorWidth / RawDepthHeightWithSpecialRatio;
            const int FilledZeroMargin = (RawDepthHeight - RawDepthHeightWithSpecialRatio) / 2;

            // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
            // not need access to the individual rgba components.
            fixed (byte* ptrColorPixels = colorImagePixels)
            {
                int* rawColorPixels = (int*)ptrColorPixels;

                Parallel.For(
                    FilledZeroMargin,
                    KinectSettings.DEPTH_HEIGHT - FilledZeroMargin,
                    y =>
                    {
                        int destIndex = y * KinectSettings.DEPTH_WIDTH;

                        for (int x = 0; x < KinectSettings.DEPTH_WIDTH; ++x, ++destIndex)
                        {
                            int srcX = (int)(x * factor);
                            int srcY = (int)(y * factor);
                            int sourceColorIndex = (srcY * KinectSettings.COLOR_WIDTH) + srcX;

                            this.resampledColorImagePixels[destIndex] = rawColorPixels[sourceColorIndex];
                        }
                    });
            }

            resampledColorFrame.CopyPixelDataFrom(this.resampledColorImagePixels);
            return resampledColorFrame;
        }

        public void Reset()
        {
            finder.ResetCameraPoseFinder();
        }

        /// <summary>
        /// This is used to set the reference frame.
        /// </summary>
        /// <param name="pose">The pose to use.</param>
        private void SetReferenceFrame(Matrix4 pose)
        {
            var smooth = volume.Engine.DepthProcessor.SmoothedDepthFloatFrame;
            // Get the saved pose view by raycasting the volume
            this.volume.Reconstruction.CalculatePointCloudAndDepth(engine.PointCloudCalculator.RaycastPointCloudFrame, smooth, null, pose);

            // Set this as the reference frame for the next call to AlignDepthFloatToReconstruction
            this.volume.Reconstruction.SetAlignDepthFloatToReconstructionReferenceFrame(smooth);
        }
    }
}
