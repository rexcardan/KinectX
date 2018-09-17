using KinectX.Fusion;
using KinectX.Meta;
using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Processors
{
    public class FusionDepthProcessor
    {
        private Engine engine;

        /// <summary>
        /// Lock object for raw pixel access
        /// </summary>
        private object rawDataLock = new object();

        /// <summary>
        /// Intermediate storage for the depth float data converted from depth image frame
        /// </summary>
        public FusionFloatImageFrame DepthFloatFrame { get; set; }

        /// <summary>
        /// Intermediate storage for the smoothed depth float image frame
        /// </summary>
        public FusionFloatImageFrame SmoothedDepthFloatFrame { get; set; }


        public FusionDepthProcessor(Engine engine)
        {
            this.engine = engine;
            DepthFloatFrame = new FusionFloatImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
            SmoothedDepthFloatFrame = new FusionFloatImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
        }

        /// <summary>
        /// Maximum residual alignment energy where tracking is still considered successful
        /// </summary>
        public int SmoothingKernelWidth { get; set; } = 1; // 0=just copy, 1=3x3, 2=5x5, 3=7x7, here we create a 3x3 kernel

        /// <summary>
        /// Maximum residual alignment energy where tracking is still considered successful
        /// </summary>
        public float SmoothingDistanceThreshold { get; set; } = 0.04f; // 4cm, could use up to around 0.1f;

        public FusionFloatImageFrame DepthToDepthFloatFrame(ushort[] depthPixels)
        {
            if (engine.FusionVolume != null && engine.FusionVolume.Reconstruction != null)
            {
                // Lock the depth operations
                lock (this.rawDataLock)
                {
                    var recon = engine.FusionVolume.Reconstruction;
                    recon.DepthToDepthFloatFrame(
                        depthPixels,
                        DepthFloatFrame,
                        engine.FusionVolume.MinDepthClip,
                        engine.FusionVolume.MaxDepthClip,
                        engine.FusionVolume.MirrorDepth);
                    return DepthFloatFrame;
                }
            }
            return new FusionFloatImageFrame(KinectSettings.DEPTH_WIDTH, KinectSettings.DEPTH_HEIGHT);
        }

        public FusionFloatImageFrame SmoothDepthFloatFrame(FusionFloatImageFrame depthFloatFrame)
        {
            // Lock the depth operations
            lock (this.rawDataLock)
            {
                var recon = engine.FusionVolume.Reconstruction;
                recon.SmoothDepthFloatFrame(
                    depthFloatFrame,
                    SmoothedDepthFloatFrame,
                    SmoothingKernelWidth,
                    SmoothingDistanceThreshold);
                return SmoothedDepthFloatFrame;
            }
        }
    }
}
