namespace KinectX.Fusion.Helpers
{
    public class Constants
    {
        /// <summary>
        /// How many frames after starting tracking will will wait before starting to store
        /// image frames to the pose finder database. Here we set 45 successful frames (1.5s).
        /// </summary>
        public const int MinSuccessfulTrackingFramesForCameraPoseFinder = 45;

        /// <summary>
        /// How many frames after starting tracking will will wait before starting to store
        /// image frames to the pose finder database. Here we set 200 successful frames (~7s).
        /// </summary>
        public const int MinSuccessfulTrackingFramesForCameraPoseFinderAfterFailure = 200;

        /// <summary>
        /// Here we set a low limit on the residual alignment energy, below which we reject a tracking
        /// success report from AlignPointClouds and believe it to have failed. This can typically be around 0.
        /// </summary>
        public const float MinAlignPointCloudsEnergyForSuccess = 0.0f;

        /// <summary>
        /// Here we set a high limit on the maximum residual alignment energy where we consider the tracking
        /// with AlignPointClouds to have succeeded. Typically this value would be around 0.005f to 0.006f.
        /// (Lower residual alignment energy after relocalization is considered better.)
        /// </summary>
        public const float MaxAlignPointCloudsEnergyForSuccess = 0.006f;

        /// <summary>
        /// Here we set a high limit on the maximum residual alignment energy where we consider the tracking
        /// to have succeeded. Typically this value would be around 0.2f to 0.3f.
        /// (Lower residual alignment energy after tracking is considered better.)
        /// </summary>
        public const float MaxAlignToReconstructionEnergyForSuccess = 0.27f;

        /// <summary>
        /// Here we set a low limit on the residual alignment energy, below which we reject a tracking
        /// success report and believe it to have failed. Typically this value would be around 0.005f, as
        /// values below this (i.e. close to 0 which is perfect alignment) most likely come from frames
        /// where the majority of the image is obscured (i.e. 0 depth) or mismatched (i.e. similar depths
        /// but different scene or camera pose).
        /// </summary>
        public const float MinAlignToReconstructionEnergyForSuccess = 0.005f;

        /// <summary>
        /// Frame interval we calculate the deltaFromReferenceFrame 
        /// </summary>
        public const int DeltaFrameCalculationInterval = 2;

        /// <summary>
        /// Max tracking error count, will reset the reconstruction if tracking errors
        /// reach the number
        /// </summary>
        public const int MaxTrackingErrors = 100;

        /// <summary>
        /// Maximum translation threshold between successive poses when using AlignPointClouds
        /// </summary>
        public const float MaxTranslationDeltaAlignPointClouds = 0.3f; // 0.15 - 0.3m per frame typical

        /// <summary>
        /// Maximum rotation threshold between successive poses when using AlignPointClouds
        /// </summary>
        public const float MaxRotationDeltaAlignPointClouds = 20.0f; // 10-20 degrees per frame typical
    }
}
