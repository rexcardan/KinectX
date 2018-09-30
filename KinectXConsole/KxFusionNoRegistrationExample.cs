using KinectX.Data;
using KinectX.Data.Listeners;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    public class KxFusionNoRegistrationExample
    {
        public static void Run(string[] args)
        {
            var fusion = new Engine();
            FusionVolume.VoxelsPerMeter = 256;
            FusionVolume.VoxelsX = 384;
            FusionVolume.VoxelsY = 384;
            FusionVolume.VoxelsZ = 384;
            //Offset volume away from Kinect center (so you actually get some data)
            VolumeResetter.TranslateResetPoseByMinDepthThreshold = true;
            fusion.DataIntegrator.CaptureColor = false;
            var listener = fusion.StartFrameListener<LiveFrameListener>();
            fusion.Scanner.Scan(10);
            fusion.RenderController.RenderReconstructionAsMat();
            fusion.FusionVolume.Renderer.RenderReconstruction();
            fusion.MeshExporter.ExportVolume(@"scan.ply");
            Console.Read();
        }
    }
}
