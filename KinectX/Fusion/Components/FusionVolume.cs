using KinectX.Fusion.Helpers;
using Microsoft.Kinect.Fusion;
using System;

namespace KinectX.Fusion.Components
{
    public class FusionVolume
    {
        public bool MirrorDepth { get; set; } = true;

        /// <summary>
        /// Minimum depth distance threshold in meters. Depth pixels below this value will be
        /// returned as invalid (0). Min depth must be positive or 0.
        /// </summary>
        public float MinDepthClip { get; set; } = FusionDepthProcessor.DefaultMinimumDepth;

        /// <summary>
        /// Maximum depth distance threshold in meters. Depth pixels above this value will be
        /// returned as invalid (0). Max depth must be greater than 0.
        /// </summary>
        public float MaxDepthClip { get; set; } = FusionDepthProcessor.DefaultMaximumDepth;

        /// <summary>
        /// The reconstruction volume voxel density in voxels per meter (vpm)
        /// 1000mm / 256vpm = ~3.9mm/voxel
        /// </summary>
        public static float VoxelsPerMeter { get; set; } = 128f;

        /// <summary>
        /// The reconstruction volume voxel resolution in the X axis
        /// At a setting of 256vpm the volume is 384 / 256 = 1.5m wide
        /// </summary>
        public static int VoxelsX { get; set; } = (int)VoxelsPerMeter;//1m;

        /// <summary>
        /// The reconstruction volume voxel resolution in the Y axis
        /// At a setting of 256vpm the volume is 384 / 256 = 1.5m high
        /// </summary>
        public static int VoxelsY { get; set; } = (int)(VoxelsPerMeter * 2.5);

        /// <summary>
        /// The reconstruction volume voxel resolution in the Z axis
        /// At a setting of 256vpm the volume is 384 / 256 = 1.5m deep
        /// </summary>
        public static int VoxelsZ { get; set; } = (int)(VoxelsPerMeter / 2);//384;

        /// <summary>
        /// The reconstruction volume processor type. This parameter sets whether AMP or CPU processing
        /// is used. Note that CPU processing will likely be too slow for real-time processing.
        /// </summary>
        public const ReconstructionProcessor ProcessorType = ReconstructionProcessor.Amp;

        /// <summary>
        /// The zero-based device index to choose for reconstruction processing if the 
        /// ReconstructionProcessor AMP options are selected.
        /// Here we automatically choose a device to use for processing by passing -1, 
        /// </summary>
        public const int DeviceToUse = -1;
        private VolumeResetter resetter;

        /// <summary>
        /// The transformation between the world and camera view coordinate system
        /// </summary>
        public Matrix4 WorldToCameraTransform { get; set; }

        /// <summary>
        /// The default between the world and volume coordinate system
        /// </summary>
        public Matrix4 DefaultWorldToVolumeTransform { get; set; }

        /// <summary>
        /// The actual volume element
        /// </summary>
        public ColorReconstruction Reconstruction { get; internal set; }

        public FusionVolume(Engine e) : this(e, Matrix4.Identity)
        {
        }

        public FusionVolume(Engine e, Matrix4 startingWorldToCameraTx)
        {
            this.Engine = e;
            ReconstructionParameters volParam = new ReconstructionParameters(FusionVolume.VoxelsPerMeter, FusionVolume.VoxelsX, FusionVolume.VoxelsY, FusionVolume.VoxelsZ);
            WorldToCameraTransform = startingWorldToCameraTx;
            this.Reconstruction = ColorReconstruction.FusionCreateReconstruction(volParam, ProcessorType, DeviceToUse, WorldToCameraTransform);
            this.DefaultWorldToVolumeTransform = this.Reconstruction.GetCurrentWorldToVolumeTransform();
            Renderer = new VolumeRenderer(e);
            resetter = new VolumeResetter();
            //ResetReconstruction(0.4f, 0.10f);
          //  this.DefaultWorldToVolumeTransform = Matrix4.Identity;
            this.resetter.ResetReconstruction(this, startingWorldToCameraTx);
        }


        public bool RecreateReconstruction(Matrix4 startingWorldToCameraTx) { return resetter.RecreateReconstruction(this, startingWorldToCameraTx); }
        public void ResetReconstruction(Matrix4 startingWorldToCameraTx) { resetter.ResetReconstruction(this, startingWorldToCameraTx); }
        public void ResetReconstruction(float inferiorExtentMeters, float posteriorExtentMeters)
        {
            resetter.WorldVolumeYShift = VoxelsPerMeter * inferiorExtentMeters;
            resetter.WorldVolumeZShift = VoxelsPerMeter * posteriorExtentMeters;
            resetter.ResetReconstruction(this, WorldToCameraTransform);
        }

        public VolumeRenderer Renderer { get; private set; }
        public Engine Engine { get; private set; }

        #region EVENTS
        public event VolumeResetHandler VolumeReset;
        public delegate void VolumeResetHandler(object sender, EventArgs e);

        /// <summary>
        /// This is called by the volume recreation class
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnVolumeReset(EventArgs e)
        {
            if (VolumeReset != null)
                VolumeReset(this, e);
        }
        #endregion
    }
}
