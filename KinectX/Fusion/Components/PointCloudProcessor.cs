using KinectX.Meta;
using Microsoft.Kinect.Fusion;

namespace KinectX.Fusion.Components
{
    public class PointCloudProcessor
    {

        /// <summary>
        /// Maximum residual alignment energy where tracking is still considered successful
        /// </summary>
        private const int SmoothingKernelWidth = 1; // 0=just copy, 1=3x3, 2=5x5, 3=7x7, here we create a 3x3 kernel

        /// <summary>
        /// Maximum residual alignment energy where tracking is still considered successful
        /// </summary>
        private const float SmoothingDistanceThreshold = 0.04f; // 4cm, could use up to around 0.1f;

        /// <summary>
        /// Intermediate storage for the depth float data following smoothing
        /// </summary>
        public FusionFloatImageFrame DownsampledSmoothDepthFloatFrame { get; set; }

        /// <summary>
        /// Calculated point cloud frame from input depth
        /// </summary>
        public FusionPointCloudImageFrame DepthPointCloudFrame { get; set; }

        /// <summary>
        /// Calculated point cloud frame from image integration
        /// </summary>
        public FusionPointCloudImageFrame RaycastPointCloudFrame { get; set; }

        public FusionPointCloudImageFrame DownsampledRaycastPointCloudFrame { get; set; }
        public FusionPointCloudImageFrame DownsampledDepthPointCloudFrame { get; set; }

        private Engine engine;
        private ColorReconstruction volume;

        public PointCloudProcessor(Engine e)
        {
            this.engine = e;
            var resampler = e.Resampler;
            this.volume = e.FusionVolume.Reconstruction;
            DownsampledSmoothDepthFloatFrame = new FusionFloatImageFrame(resampler.DownsampledWidth, resampler.DownsampledHeight);
            DownsampledRaycastPointCloudFrame = new FusionPointCloudImageFrame(resampler.DownsampledWidth, resampler.DownsampledHeight);
            DownsampledDepthPointCloudFrame = new FusionPointCloudImageFrame(resampler.DownsampledWidth, resampler.DownsampledHeight);
            DepthPointCloudFrame = new FusionPointCloudImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
            RaycastPointCloudFrame = new FusionPointCloudImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
        }

        public void CreateSmoothDepthCloud(FusionFloatImageFrame downsampledDepthFloatFrame)
        {
            // Smooth the depth frame
            this.volume.SmoothDepthFloatFrame(downsampledDepthFloatFrame, this.DownsampledSmoothDepthFloatFrame, SmoothingKernelWidth, SmoothingDistanceThreshold);
            // Calculate point cloud from the smoothed frame
            FusionDepthProcessor.DepthFloatFrameToPointCloud(this.DownsampledSmoothDepthFloatFrame, DownsampledDepthPointCloudFrame);
        }

        public void RaycastPointCloud(Matrix4 calculatedCameraPose)
        {
            // Get the saved pose view by raycasting the volume from the current camera pose
            this.volume.CalculatePointCloud(DownsampledRaycastPointCloudFrame, calculatedCameraPose);
        }

        public void UpdateDepthPointCloud(FusionFloatImageFrame smooth)
        {
            FusionDepthProcessor.DepthFloatFrameToPointCloud(smooth, DepthPointCloudFrame);
        }
    }
}