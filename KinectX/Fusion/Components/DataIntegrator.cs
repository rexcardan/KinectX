using Microsoft.Kinect.Fusion;

namespace KinectX.Fusion.Components
{
    public class DataIntegrator
    {
        private Engine engine;

        public bool IntegrationPaused { get; set; }

        /// <summary>
        /// Image integration weight
        /// </summary>
        public short IntegrationWeight { get; set; } = 200;

        /// <summary>
        /// The frame interval where we integrate color.
        /// Capturing color has an associated processing cost, so we do not have to capture every frame here.
        /// </summary>
        public int ColorIntegrationInterval { get; set; } = 1;

        /// <summary>
        /// Capture, integrate and display color when true
        /// </summary>
        public bool CaptureColor { get; set; } = true;
        public bool ColorCaptured { get; private set; }

        public DataIntegrator(Engine e)
        {
            this.engine = e;
        }

        public bool IntegrateData()
        {
            // Color may opportunistically be available here - check
            bool colorAvailable = engine.FrameListener.ColorReadyEvent.WaitOne(100);
            bool integrateData = engine.CameraTracker.IsClearToIntegrate;

            // Integrate the frame to volume
            if (integrateData)
            {
                bool integrateColor = engine.CameraTracker.ProcessedFrameCount % ColorIntegrationInterval == 0 && colorAvailable;

                // Reset this flag as we are now integrating data again
                engine.CameraTracker.TrackingHasFailedPreviously = false;
                var fdl = engine.FrameListener;
                var depth = fdl.GetDepthImagePixels();
                var color = fdl.GetColorImagePixels();
                //var dff = engine.DepthProcessor.DepthToDepthFloatFrame(depth);
                //engine.DepthProcessor.SmoothDepthFloatFrame(dff);

                if (CaptureColor && integrateColor)
                {  
                    // Pre-process color
                   var resampleColorFrame = engine.ColorProcessor.MapColorToDepth(
                       depth,
                       color,
                       fdl.CoordinateMapper,
                       engine.FusionVolume.MirrorDepth);

                    // Integrate color and depth
                    engine.FusionVolume.Reconstruction.IntegrateFrame(
                        engine.DepthProcessor.DepthFloatFrame,
                        engine.ColorProcessor.ResampledColorFrameDepthAligned,
                        IntegrationWeight,
                        FusionDepthProcessor.DefaultColorIntegrationOfAllAngles,
                        engine.FusionVolume.WorldToCameraTransform);

                    // Flag that we have captured color
                    ColorCaptured = true;
                }
                else
                {
                    // Just integrate depth
                    engine.FusionVolume.Reconstruction.IntegrateFrame(
                        engine.DepthProcessor.DepthFloatFrame,
                        IntegrationWeight,
                        engine.FusionVolume.WorldToCameraTransform);
                }

                // Reset color ready event
                engine.FrameListener.ColorReadyEvent.Reset();
            }

            return colorAvailable;
        }
    }
}
