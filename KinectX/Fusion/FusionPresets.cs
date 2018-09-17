using KinectX.Extensions;
using Microsoft.Kinect.Fusion;

namespace KinectX.Fusion
{
    public class FusionPresets
    {
        public static ReconstructionParameters HighResReconstruction = new ReconstructionParameters(512, 512, 512, 512);
        public static ReconstructionParameters DefaultReconstruction = new ReconstructionParameters(256, 384, 384, 384);
        public static ReconstructionParameters SideReconstruction = new ReconstructionParameters(256, 512, 512, 384);
        public static ReconstructionParameters LargeVolumeReconstruction = new ReconstructionParameters(128, 256, 256, 384);
        public static ReconstructionParameters TableTop = new ReconstructionParameters(256, 256, 512, 128);

        public static Matrix4 FusionToCamSpace
        {
            get
            {
                var mat = Matrix4.Identity;
                mat.M14 = -0.00f; //slight X shift 1 cm
                mat.M22 = -1f;
                mat.M24 = 0f; //-0.02; //-0.025 2.5 cm z shift

                return mat;
            }
        }

        public static Matrix4 MetersToMM
        {
            get
            {
                var mat = Matrix4.Identity;
                mat = mat.Scale(1000);
                return mat;

            }
        }

        public static Matrix4 FlipXY
        {
            get
            {
                var mat = Matrix4.Identity;
                mat.M11 = -1;
                mat.M22 = -1;
                return mat;

            }
        }

        public static Matrix4 FusionShift
        {
            get
            {
                var mat = Matrix4.Identity;
                mat.M14 = -0.01f;
                mat.M24 = 0.01f;
                mat.M34 = -0.02f;
                return mat;
            }
        }

        /// <summary>
        /// Width of raw depth stream
        /// </summary>
        public const int RawDepthWidth = 512;

        /// <summary>
        /// Height of raw depth stream
        /// </summary>
        public const int RawDepthHeight = 424;
    }
}

