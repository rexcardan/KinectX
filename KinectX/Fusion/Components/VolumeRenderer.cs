using KinectX.Meta;
using Microsoft.Kinect.Fusion;

namespace KinectX.Fusion.Components
{
    public class VolumeRenderer
    {
        private Engine engine;

        /// <summary>
        /// Shaded surface frame from shading point cloud frame
        /// </summary>
        public FusionColorImageFrame ShadedSurfaceFrame { get; set; }

        /// <summary>
        /// Shaded surface normals frame from shading point cloud frame
        /// </summary>
        public FusionColorImageFrame ShadedSurfaceNormalsFrame { get; set; }

        public bool UseVolumeGraphics { get; set; } = true;
        public Matrix4 WorldToBGRTransform { get; set; }
        public bool ViewChanged { get; internal set; }
        public bool KinectView { get; private set; } = true;
        public bool DisplayNormals { get; private set; } = false;

        /// <summary>
        /// This is used when it is not in "KinectView" mode
        /// </summary>
        public Matrix4 RenderWorldToCameraMatrix { get; set; } = Matrix4.Identity;

        public VolumeRenderer(Engine e)
        {
            this.engine = e;
            ShadedSurfaceFrame = new FusionColorImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
            ShadedSurfaceNormalsFrame = new FusionColorImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
        }

        public void ResetWorldToBGR()
        {
            var fusionVolume = engine.FusionVolume;
            // Map world X axis to blue channel, Y axis to green channel and Z axis to red channel,
            // normalizing each to the range [0, 1]. We also add a shift of 0.5 to both X,Y channels
            // as the world origin starts located at the center of the front face of the volume,
            // hence we need to map negative x,y world vertex locations to positive color values.
            var worldToBGRTx = Matrix4.Identity;
            worldToBGRTx.M11 = FusionVolume.VoxelsPerMeter / FusionVolume.VoxelsX;
            worldToBGRTx.M22 = FusionVolume.VoxelsPerMeter / FusionVolume.VoxelsY;
            worldToBGRTx.M33 = FusionVolume.VoxelsPerMeter / FusionVolume.VoxelsZ;
            worldToBGRTx.M41 = 0.5f;
            worldToBGRTx.M42 = 0.5f;
            worldToBGRTx.M44 = 1.0f;

            WorldToBGRTransform = worldToBGRTx;
        }

        /// <summary>
        /// Render the reconstruction
        /// </summary>
        public void RenderReconstruction()
        {
            var fusionVolume = engine.FusionVolume;
            if (null == fusionVolume.Reconstruction || engine.MeshExporter.IsSavingMesh || null == engine.PointCloudCalculator.RaycastPointCloudFrame
                || null == ShadedSurfaceFrame || null == ShadedSurfaceNormalsFrame)
            {
                return;
            }

            var pc = engine.PointCloudCalculator;

            // If KinectView option has been set, use the worldToCameraTransform, else use the virtualCamera transform
            Matrix4 cameraView = this.KinectView ? fusionVolume.WorldToCameraTransform : RenderWorldToCameraMatrix;

            if (engine.DataIntegrator.CaptureColor)
            {
                fusionVolume.Reconstruction.CalculatePointCloud(pc.RaycastPointCloudFrame, ShadedSurfaceFrame, cameraView);
            }
            else
            {
                fusionVolume.Reconstruction.CalculatePointCloud(pc.RaycastPointCloudFrame, cameraView);

                // Shade point cloud frame for rendering
                FusionDepthProcessor.ShadePointCloud(
                    pc.RaycastPointCloudFrame,
                    cameraView,
                    WorldToBGRTransform,
                    DisplayNormals ? null : ShadedSurfaceFrame,
                    DisplayNormals ? ShadedSurfaceNormalsFrame : null);
            }

            var renderFrame = engine.DataIntegrator.CaptureColor ? 
                ShadedSurfaceFrame : (DisplayNormals ? ShadedSurfaceNormalsFrame : ShadedSurfaceFrame);

            engine.RenderController.RenderReconstruction(renderFrame);
        }
    }
}
