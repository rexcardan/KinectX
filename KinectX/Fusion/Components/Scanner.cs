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
        public void Scan(int numOfFrames)
        {
            var frames = 0;
            var mdl = engine.FrameListener;

            while (frames < numOfFrames)
            {
                var depthReady = mdl.DepthReadyEvent.WaitOne(10000); // At most 10 sec

                if (depthReady)
                {
                    var depth = mdl.GetDepthImagePixels();
                    var df = engine.DepthProcessor.DepthToDepthFloatFrame(depth);
                    engine.RenderController.RendorDepth(df);
                }

                var colorReady = mdl.ColorReadyEvent.WaitOne(10000); // At most 10 sec
                if (colorReady)
                {
                    engine.RenderController.RenderColor(mdl.GetColorImagePixels());
                    mdl.ColorReadyEvent.Reset();
                }

                if (depthReady && colorReady && engine.FusionVolume != null)
                {
                    int errors = 0;

                    ////Try to track
                    _logger.Info($"Tracking camera...");
                    engine.CameraTracker.TrackCamera(mdl.GetColorImagePixels(), mdl.GetDepthImagePixels());
                    errors = engine.CameraTracker.TrackingErrorCount;
                    if (errors > 0) { _logger.Warn($"{errors} errors while tracking!"); }
                    else if(engine.CameraTracker.IsClearToIntegrate)
                    {
                        _logger.Info($"Integrating frame {frames + 1}/{numOfFrames}...");
                        engine.DataIntegrator.IntegrateData();
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
                mdl.DepthReadyEvent.Reset();
                mdl.ColorReadyEvent.Reset();
            }

        }
    }
}
