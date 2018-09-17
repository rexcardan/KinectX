using KinectX.Fusion.Helpers;
using Microsoft.Kinect.Fusion;
using NLog;
using System;
using System.Globalization;

namespace KinectX.Fusion.Components
{
    public class CameraTracker
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Alignment energy from AlignDepthFloatToReconstruction for current frame 
        /// </summary>
        private float AlignmentEnergy { get; set; }

        private Engine engine;

        public int SuccessfulFrameCount { get; private set; }
        public int TrackingErrorCount { get; private set; }
        public bool TrackingHasFailedPreviously { get; set; }
        public int ProcessedFrameCount { get; private set; }
        public bool AutoResetReconstructionWhenLost { get; private set; }
        public bool CurrentTrackingFailed { get; private set; }
        public bool IsClearToIntegrate
        {
            get
            {
                // Don't integrate depth data into the volume if:
                // 1) tracking failed
                // 2) camera pose finder is off and we have paused capture
                // 3) camera pose finder is on and we are still under the m_cMinSuccessfulTrackingFramesForCameraPoseFinderAfterFailure
                //    number of successful frames count.
                var finderAvailable = engine.PoseFinder.IsAvailable();
                return !CurrentTrackingFailed && !engine.DataIntegrator.IntegrationPaused &&
                    (!finderAvailable || (finderAvailable && !(TrackingHasFailedPreviously && SuccessfulFrameCount < Constants.MinSuccessfulTrackingFramesForCameraPoseFinderAfterFailure)));
            }
        }

        public CameraTracker(Engine e)
        {
            this.engine = e;
        }

        /// <summary>
        /// Track the camera pose
        /// </summary>
        public void TrackCamera(byte[] colorPixels, ushort[] depthPixels)
        {
            bool calculateDeltaFrame = false;// this.ProcessedFrameCount % Constants.DeltaFrameCalculationInterval == 0;
            bool trackingSucceeded = false;

            // Get updated camera transform from image alignment
            Matrix4 calculatedCameraPos = engine.FusionVolume.WorldToCameraTransform;

            // Here we can either call TrackCameraAlignDepthFloatToReconstruction or TrackCameraAlignPointClouds
            // The TrackCameraAlignPointClouds function typically has higher performance with the camera pose finder 
            // due to its wider basin of convergence, enabling it to more robustly regain tracking from nearby poses
            // suggested by the camera pose finder after tracking is lost.
            if (engine.PoseFinder.AutoFindCameraPoseWhenLost)
            {
                // Track using AlignPointClouds
                trackingSucceeded = this.TrackCameraAlignPointClouds(depthPixels, ref calculateDeltaFrame, ref calculatedCameraPos);
            }
            else
            {
                // Track using AlignDepthFloatToReconstruction
                //TODO Implement this next function
                trackingSucceeded = this.TrackCameraAlignDepthFloatToReconstruction(calculateDeltaFrame, ref calculatedCameraPos);
            }

            if (!trackingSucceeded && 0 != this.SuccessfulFrameCount)
            {
                this.SetTrackingFailed();

                if (engine.PoseFinder == null || !engine.PoseFinder.AutoFindCameraPoseWhenLost)
                {
                    logger.Log(LogLevel.Warn, "Camera tracking failed");
                }
                else
                {
                    // Here we try to find the correct camera pose, to re-localize camera tracking.
                    // We can call either the version using AlignDepthFloatToReconstruction or the
                    // version using AlignPointClouds, which typically has a higher success rate.
                    // trackingSucceeded = this.FindCameraPoseAlignDepthFloatToReconstruction();
                    var depthFloatFrame = engine.DepthProcessor.DepthToDepthFloatFrame(depthPixels);
                    trackingSucceeded = engine.PoseFinder.FindCameraPoseAlignPointClouds(colorPixels, depthFloatFrame);

                    if (!trackingSucceeded)
                    {
                        logger.Log(LogLevel.Warn, "Camera tracking failed");
                    }
                }
            }
            else
            {
                if (this.TrackingHasFailedPreviously)
                {
                    logger.Log(LogLevel.Warn, "Kinect Fusion camera tracking RECOVERED! Residual energy=" + string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", this.AlignmentEnergy));
                }

                engine.DeltaCalculator.UpdateAlignDeltas();

                SetTrackingSucceeded();

                engine.FusionVolume.WorldToCameraTransform = calculatedCameraPos;
            }

            if (AutoResetReconstructionWhenLost && !trackingSucceeded
                && this.TrackingErrorCount >= Constants.MaxTrackingErrors)
            {
                // Bad tracking
                logger.Log(LogLevel.Warn, "Reset volume. Bad tracking.");
                OnMaxTrackingErrorsReached(EventArgs.Empty);
            }

            //TODO Subscribe to tracking succeeded event and do this on main window
            //if (trackingSucceeded)
            //{
            //    if (this.kinectView)
            //    {
            //        _dispatcher.BeginInvoke((Action)(() => this.UpdateVirtualCameraTransform()));
            //    }
            //    else
            //    {
            //        // Just update the frustum
            //        _dispatcher.BeginInvoke((Action)(() => this.virtualCamera.UpdateFrustumTransformMatrix4(this.worldToCameraTransform)));
            //    }

            //    // Increase processed frame counter
            //    this.ProcessedFrameCount++;
            //}
        }

        /// <summary>
        /// Track camera pose by aligning depth float image with reconstruction volume
        /// </summary>
        /// <param name="calculateDeltaFrame">Flag to calculate the delta frame.</param>
        /// <param name="calculatedCameraPos">The calculated camera position.</param>
        /// <returns>Returns true if tracking succeeded, false otherwise.</returns>
        private bool TrackCameraAlignDepthFloatToReconstruction(bool calculateDeltaFrame, ref Matrix4 calculatedCameraPos)
        {
            bool trackingSucceeded = false;
            float alignmentEnergy;

            // Note that here we only calculate the deltaFromReferenceFrame every 
            // DeltaFrameCalculationInterval frames to reduce computation time
            if (calculateDeltaFrame)
            {

                trackingSucceeded = engine.FusionVolume.Reconstruction.AlignDepthFloatToReconstruction(
                    engine.DepthProcessor.DepthFloatFrame,
                    FusionDepthProcessor.DefaultAlignIterationCount,
                    engine.DeltaCalculator.DeltaFromReferenceFrame,
                    out alignmentEnergy,
                    engine.FusionVolume.WorldToCameraTransform);
            }
            else
            {
                // Don't bother getting the residual delta from reference frame to cut computation time
                trackingSucceeded = engine.FusionVolume.Reconstruction.AlignDepthFloatToReconstruction(
                    engine.DepthProcessor.DepthFloatFrame,
                    FusionDepthProcessor.DefaultAlignIterationCount,
                    null,
                    out alignmentEnergy,
                    engine.FusionVolume.WorldToCameraTransform);
            }

            //Store for future reporting
            AlignmentEnergy = alignmentEnergy;

            if (!trackingSucceeded || alignmentEnergy > Constants.MaxAlignToReconstructionEnergyForSuccess || (alignmentEnergy <= Constants.MinAlignToReconstructionEnergyForSuccess && SuccessfulFrameCount > 0))
            {
                trackingSucceeded = false;
            }
            else
            {
                // Tracking succeeded, get the updated camera pose
                calculatedCameraPos = engine.FusionVolume.Reconstruction.GetCurrentWorldToCameraTransform();
            }

            return trackingSucceeded;
        }

        /// <summary>
        /// Set variables if camera tracking succeeded
        /// </summary>
        public void SetTrackingFailed()
        {
            // Clear successful frame count and increment the track error count
            this.CurrentTrackingFailed = true;
            this.TrackingHasFailedPreviously = true;
            this.TrackingErrorCount++;
            this.SuccessfulFrameCount = 0;
            OnTrackingFailed(EventArgs.Empty);
        }

        /// <summary>
        /// Set variables if camera tracking succeeded
        /// </summary>
        public void SetTrackingSucceeded()
        {
            // Clear track error count and increment the successful frame count
            CurrentTrackingFailed = false;
            TrackingErrorCount = 0;
            SuccessfulFrameCount++;
            OnTrackingSuccess(EventArgs.Empty);
        }

        /// <summary>
        /// Reset tracking variables
        /// </summary>
        public void ResetTracking()
        {
            CurrentTrackingFailed = false;
            TrackingHasFailedPreviously = false;
            TrackingErrorCount = 0;
            SuccessfulFrameCount = 0;

            if (null != engine.PoseFinder)
            {
                engine.PoseFinder.Reset();
            }

            //TODO Subscribe and turn pause integration to false
            OnTrackingReset(EventArgs.Empty);
        }

        #region EVENTS
        public event TrackingResetHandler TrackingReset;
        public delegate void TrackingResetHandler(object sender, EventArgs e);
        protected virtual void OnTrackingReset(EventArgs e)
        {
            if (TrackingReset != null)
                TrackingReset(this, e);
        }

        /// <summary>
        /// Tracking failed event
        /// </summary>
        /// 
        public event TrackingFailedHandler TrackingFailed;
        public delegate void TrackingFailedHandler(object sender, EventArgs e);
        protected virtual void OnTrackingFailed(EventArgs e)
        {
            if (TrackingFailed != null)
                TrackingFailed(this, e);
        }


        /// <summary>
        /// Tracking succeeded event
        /// </summary>
        public event TrackingSuccessHandler TrackingSucceeded;
        public delegate void TrackingSuccessHandler(object sender, EventArgs e);
        protected virtual void OnTrackingSuccess(EventArgs e)
        {
            if (TrackingFailed != null)
                TrackingSucceeded(this, e);
        }

        public event MaxTrackingErrorsReachedHandler MaxTrackingErrorsReached;
        public delegate void MaxTrackingErrorsReachedHandler(object sender, EventArgs e);
        protected virtual void OnMaxTrackingErrorsReached(EventArgs e)
        {
            if (MaxTrackingErrorsReached != null)
                MaxTrackingErrorsReached(this, e);
        }
        #endregion

        /// <summary>
        /// Track camera pose using AlignPointClouds
        /// </summary>
        /// <param name="calculateDeltaFrame">A flag to indicate it is time to calculate the delta frame.</param>
        /// <param name="calculatedCameraPose">The calculated camera pose.</param>
        /// <returns>Returns true if tracking succeeded, false otherwise.</returns>
        private bool TrackCameraAlignPointClouds(ushort[] depthPixels, ref bool calculateDeltaFrame, ref Matrix4 calculatedCameraPose)
        {
            bool trackingSucceeded = false;

            //Resample at lower resolution
            var downsampledDepthFloatFrame = engine.Resampler.GetDefaultFloatFrame();
            engine.Resampler.DownsampleDepthFrameNearestNeighbor(downsampledDepthFloatFrame, depthPixels, engine.FusionVolume.MirrorDepth);

            //Smooth
            engine.PointCloudCalculator.CreateSmoothDepthCloud(downsampledDepthFloatFrame);
            //get point cloud for tracking
            engine.PointCloudCalculator.RaycastPointCloud(calculatedCameraPose);

            Matrix4 initialPose = calculatedCameraPose;

            // Note that here we only calculate the deltaFromReferenceFrame every 
            // DeltaFrameCalculationInterval frames to reduce computation time
            if (calculateDeltaFrame)
            {
                trackingSucceeded = engine.DeltaCalculator.CalculateDeltaFrame(calculatedCameraPose);
                // Set calculateDeltaFrame to false as we are rendering it here
                calculateDeltaFrame = false;
            }
            else
            {
                // Don't bother getting the residual delta from reference frame to cut computation time
                trackingSucceeded = FusionDepthProcessor.AlignPointClouds(
                    engine.PointCloudCalculator.DownsampledRaycastPointCloudFrame,
                    engine.PointCloudCalculator.DownsampledDepthPointCloudFrame,
                    FusionDepthProcessor.DefaultAlignIterationCount,
                    null,
                    ref calculatedCameraPose);
            }

            if (trackingSucceeded)
            {
                bool failed = KinectFusionHelper.CameraTransformFailed(
                    initialPose,
                    calculatedCameraPose,
                    Constants.MaxTranslationDeltaAlignPointClouds,
                    Constants.MaxRotationDeltaAlignPointClouds);

                if (failed)
                {
                    trackingSucceeded = false;
                }
            }

            return trackingSucceeded;
        }
    }
}
