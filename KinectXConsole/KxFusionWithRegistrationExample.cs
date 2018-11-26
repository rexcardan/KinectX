using KinectX.Data.Listeners;
using KinectX.Extensions;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.IO;
using KinectX.Registration;
using System;

namespace KinectXConsole
{
    public class KxFusionWithRegistrationExample
    {
        public static void Run(string[] args)
        {
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Microcube();

            //Find registration
            var xef = new Xef(@"../../../Resources/cube.xef");
            var colorCv = xef.LoadCvColorFrame(0);

            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            //Vision.DrawAruco(colorCv).Show();

            //Calculate pose
            var _3dImage = xef.LoadCVCameraSpace(5);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            var pose = kxTransform.FusionCameraPose.ToMatrix4();

            var fusion = new Engine();
            FusionVolume.VoxelsPerMeter = 128;
            FusionVolume.VoxelsX = 384;
            FusionVolume.VoxelsY = 384;
            FusionVolume.VoxelsZ = 384;

            //Start fusion volume at first pose
            fusion.InitializeFusionVolume(pose);
            VolumeResetter.TranslateResetPoseByMinDepthThreshold = false;

            fusion.DataIntegrator.CaptureColor = false;
            var listener = fusion.StartFrameListener<XefFrameListener>();
            
            //This would be where you would set your scan xef
            listener.SetXefFile(@"../../../Resources/cube.xef");
            //You need to set world to camera BEFORE scanning (if more than one XEF)
            fusion.FusionVolume.WorldToCameraTransform = pose;
            fusion.Scanner.Scan(3, false);

            fusion.RenderController.RenderReconstructionAsMat();
            fusion.FusionVolume.Renderer.RenderReconstruction();

            //Export your model in world space (it is transformed already)
            fusion.MeshExporter.ExportVolume(@"cube.ply");
            Console.Read();
        }
    }
}
