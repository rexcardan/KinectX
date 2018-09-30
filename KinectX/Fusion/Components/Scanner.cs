using KinectX.Data.Listeners;
using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Fusion.Components
{
    public class Scanner
    {
        private Engine engine;
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public Scanner(Engine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Constructs a fusion volume using the currently connected Kinect camera
        /// Call Calibrate() first.
        /// </summary>
        /// <param name="numOfFrames">the number of frames to integrate in the fusion volume</param>
        public void Scan(int numOfFrames, bool doTrackCamera = true)
        {
            var frames = 0;
            var mdl = engine.FrameListener;

            while (frames < numOfFrames)
            {
                _logger.Info($"Scanning from {frames}/{numOfFrames}...");
                var depthReady = mdl.DepthReadyEvent.WaitOne(10000); // At most 10 sec
                ushort[] depthShorts = new ushort[KinectSettings.DEPTH_PIXEL_COUNT];
                if (depthReady)
                {
                    depthShorts = mdl.GetDepthImagePixels();
                    var df = engine.DepthProcessor.DepthToDepthFloatFrame(depthShorts);
                    engine.RenderController.RendorDepth(df);
                }

                bool colorReady = false;
                byte[] colorBytes = new byte[KinectSettings.COLOR_PIXEL_COUNT * 4];

                colorReady = mdl.ColorReadyEvent.WaitOne(10000); // At most 10 sec
                if (colorReady)
                {
                    colorBytes = mdl.GetColorImagePixels();
                    engine.RenderController.RenderColor(colorBytes);
                }


                if (depthReady &&  colorReady && engine.FusionVolume != null)
                {
                    int errors = 0;

                    if (doTrackCamera)
                    {
                        ////Try to track
                        _logger.Info($"Tracking camera...");
                        engine.CameraTracker.TrackCamera(colorBytes, depthShorts);
                        errors = engine.CameraTracker.TrackingErrorCount;
                        if (errors > 0) { _logger.Warn($"{errors} errors while tracking!"); }

                    }
                    if ((errors == 0 && engine.CameraTracker.IsClearToIntegrate) || !doTrackCamera)
                    {
                        _logger.Info($"Integrating frame {frames + 1}/{numOfFrames}...");
                        engine.DataIntegrator.IntegrateData(colorBytes, depthShorts);
                        // Check to see if another depth frame is already available. 
                        // If not we have time to calculate a point cloud and render, 
                        // but if so we make sure we force a render at least every 
                        // RenderIntervalMilliseconds.
                        if (!engine.FrameListener.DepthReadyEvent.WaitOne(0) || engine.RenderController.IsRenderOverdue)
                        {
                            // Raycast and render
                            engine.FusionVolume.Renderer.RenderReconstruction();
                        }
                    }
                    else
                    {
                        _logger.Error($"No frame detected! Check camera!");
                    }
                }

                frames++;
            }

            //Reset frames
            mdl.ColorReadyEvent.Reset();
            mdl.DepthReadyEvent.Reset();
        }
    }
}
