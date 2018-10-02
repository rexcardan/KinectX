using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectX.Extensions;
using Microsoft.Kinect.Fusion;
using OpenCvSharp;

namespace KinectX.Mathematics
{
    public class KxTransform
    {

        public KxTransform(double[,] camSpaceToWorldTx, bool isInvertZNeeded = false)
        {
            if (isInvertZNeeded)
            {
                var flipZ = MatrixHelper.Identity(4);
                flipZ[2, 2] = -1;
                this.CameraSpaceToWorldTx = flipZ.Multiply(camSpaceToWorldTx);
            }
            else
            {
                this.CameraSpaceToWorldTx = camSpaceToWorldTx;
            }

            FusionToWorldTx = CameraSpaceToWorldTx.Multiply(FusionToCamSpace);
            FusionCameraPose = FusionToWorldTx.Inverse().Transpose();
            CameraPose = CameraSpaceToWorldTx.Inverse().Transpose();
        }

        public double[,] CameraSpaceToWorldTx { get; private set; }
        public double[,] FusionToWorldTx { get; private set; }
        public double[,] FusionCameraPose { get; set; }
        public double[,] CameraPose { get; set; }

        private static double[,] FusionToCamSpace
        {
            get
            {
                var mat = MatrixHelper.Identity(4);
                mat[1,1] = -1;
                return mat;
            }
        }
    }
}
