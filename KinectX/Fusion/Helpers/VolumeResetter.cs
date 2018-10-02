using KinectX.Extensions;
using KinectX.Fusion.Components;
using Microsoft.Kinect.Fusion;
using NLog;
using System;

namespace KinectX.Fusion.Helpers
{
    public class VolumeResetter
    {
        /// <summary>
        /// Parameter to translate the reconstruction based on the minimum depth setting. When set to
        /// false, the reconstruction volume +Z axis starts at the camera lens and extends into the scene.
        /// Setting this true in the constructor will move the volume forward along +Z away from the
        /// camera by the minimum depth threshold to enable capture of very small reconstruction volume
        /// by setting a non-identity world-volume transformation in the ResetReconstruction call.
        /// Small volumes should be shifted, as the Kinect hardware has a minimum sensing limit of ~0.35m,
        /// inside which no valid depth is returned, hence it is difficult to initialize and track robustly  
        /// when the majority of a small volume is inside this distance.
        /// </summary>
        public static bool TranslateResetPoseByMinDepthThreshold { get; set; } = false;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Lock object for volume re-creation and meshing
        /// </summary>
        private object volumeLock = new object();

        public float WorldVolumeYShift { get; set; }
        public float WorldVolumeZShift { get; set; }

        /// <summary>
        /// Re-create the reconstruction object
        /// </summary>
        /// <returns>Indicate success or failure</returns>
        public bool RecreateReconstruction(FusionVolume vol, Matrix4 startingWorldToCameraTx)
        {
            lock (this.volumeLock)
            {

                if (null != vol.Reconstruction)
                {
                    vol.Reconstruction.Dispose();
                    vol.Reconstruction = null;
                }

                try
                {
                    ReconstructionParameters volParam = new ReconstructionParameters(FusionVolume.VoxelsPerMeter, FusionVolume.VoxelsX, FusionVolume.VoxelsY, FusionVolume.VoxelsZ);

                    // Set the world-view transform to identity, so the world origin is the initial camera location.
                    vol.WorldToCameraTransform = startingWorldToCameraTx;

                    vol.Reconstruction = ColorReconstruction.FusionCreateReconstruction(volParam, FusionVolume.ProcessorType, FusionVolume.DeviceToUse, vol.WorldToCameraTransform);

                    vol.DefaultWorldToVolumeTransform = vol.Reconstruction.GetCurrentWorldToVolumeTransform();

                    if (VolumeResetter.TranslateResetPoseByMinDepthThreshold)
                    {
                        ResetReconstruction(vol, startingWorldToCameraTx);
                    }
                    else
                    {
                        vol.Engine.CameraTracker.ResetTracking();
                        vol.Engine.ColorProcessor.ResetColorImage();
                    }

                    vol.Renderer.ResetWorldToBGR();

                    if (vol.Engine.CubeDrawer != null) vol.Engine.CubeDrawer.UpdateVolumeCube();

                    vol.Renderer.ViewChanged = true;

                    return true;
                }
                catch (ArgumentException)
                {
                    vol.Reconstruction = null;
                    logger.Log(LogLevel.Error, "Volume resolution not appropriatate");
                }
                catch (InvalidOperationException ex)
                {
                    vol.Reconstruction = null;
                    logger.Log(LogLevel.Error, ex);
                }
                catch (DllNotFoundException)
                {
                    vol.Reconstruction = null;
                    logger.Log(LogLevel.Error, "Missing Dll prerequisite for volume reconstruction");
                }
                catch (OutOfMemoryException)
                {
                    vol.Reconstruction = null;
                    logger.Log(LogLevel.Error, "Out of memory when recreating volume");
                }

                return false;
            }
        }

        /// <summary>
        /// Reset reconstruction object to initial state
        /// </summary>
        public void ResetReconstruction(FusionVolume vol, Matrix4 startingWorldToCameraTx)
        {

            // Reset tracking error counter
            vol.Engine.CameraTracker.ResetTracking();

            // Set the world-view transform to identity, so the world origin is the initial camera location.
            vol.WorldToCameraTransform = startingWorldToCameraTx;

            // Reset volume
            if (null != vol.Reconstruction)
            {
                try
                {
                    // Translate the reconstruction volume location away from the world origin by an amount equal
                    // to the minimum depth threshold. This ensures that some depth signal falls inside the volume.
                    // If set false, the default world origin is set to the center of the front face of the 
                    // volume, which has the effect of locating the volume directly in front of the initial camera
                    // position with the +Z axis into the volume along the initial camera direction of view.
                    if (TranslateResetPoseByMinDepthThreshold)
                    {
                        Matrix4 worldToVolumeTransform = vol.DefaultWorldToVolumeTransform;

                        // Translate the volume in the Z axis by the minDepthClip distance
                        float minDist = (vol.MinDepthClip < vol.MaxDepthClip) ? vol.MinDepthClip : vol.MaxDepthClip;
                        worldToVolumeTransform.M43 -= minDist * FusionVolume.VoxelsPerMeter;

                        vol.Reconstruction.ResetReconstruction(vol.WorldToCameraTransform, worldToVolumeTransform);
                    }
                    else
                    {
                        var tx = GetRealWorldVolumeCoordinates(vol);
                        vol.Reconstruction.ResetReconstruction(vol.WorldToCameraTransform, tx);
                    }
                    vol.Engine.CameraTracker.ResetTracking();
                    vol.OnVolumeReset(EventArgs.Empty);
                }
                catch (InvalidOperationException)
                {
                    logger.Log(LogLevel.Info, "Reset failed");
                }
            }

            // Update manual reset information to status bar
            logger.Log(LogLevel.Info, "Volume reset");
        }

        private Matrix4 GetRealWorldVolumeCoordinates(FusionVolume vol)
        {
            var tx = Matrix4.Identity.Scale(FusionVolume.VoxelsPerMeter);

            //Volume box starts in top right corner. 
            //We need to shift the x off of the central axis by half voxel width
            tx.M41 = FusionVolume.VoxelsX / 2 + 0.5f; // Half and half each way
            //We need to shift top of the box off the central axis by 4/3 meters
            tx.M42 = WorldVolumeYShift;// 
            tx.M43 = FusionVolume.VoxelsZ / 2 + 0.5f; //Slightly below the couch
            return tx;
        }
    }
}
