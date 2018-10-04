using KinectX.Data.Listeners;
using KinectX.Extensions;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.Registration;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pose = KinectX.Registration.PoseFinder;

namespace KinectXConsole
{
    public class MulticameraFusionExample
    {
        static void Run(string[] args)
        {
            int NUM_SCANS_PER_CAMERA = 20;

            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Cube();

            var scans = new Dictionary<string, string>()
            {
                {@"C:\XEF\cam1_cal.xef", @"C:\XEF\cam1_scan.xef" },
                {@"C:\XEF\cam2_cal.xef", @"C:\XEF\cam2_scan.xef" }
            };

            var fusion = new Engine();
            FusionVolume.VoxelsPerMeter = 384;
            FusionVolume.VoxelsX = 384;
            FusionVolume.VoxelsY = 384;
            FusionVolume.VoxelsZ = 384;

            var firstPose = Pose.GetPoseFromXef(scans.First().Key)
                .FusionCameraPose
                .ToMatrix4();

            //Start fusion volume at first pose
            fusion.InitializeFusionVolume(firstPose);
            VolumeResetter.TranslateResetPoseByMinDepthThreshold = false;
            fusion.DataIntegrator.IntegrationWeight = 500;
            fusion.DataIntegrator.CaptureColor = false;
            var listener = fusion.StartFrameListener<XefFrameListener>();

            foreach (var scan in scans)
            {
                var scanXef = scan.Value;
                var pose = Pose.GetPoseFromXef(scan.Key)
                    .FusionCameraPose
                    .ToMatrix4();
                //This would be where you would set your scan xef
                listener.SetXefFile(scanXef);
                //You need to set world to camera BEFORE scanning (if more than one XEF)
                fusion.FusionVolume.WorldToCameraTransform = pose;
                fusion.Scanner.Scan(NUM_SCANS_PER_CAMERA, false);
            }

            fusion.RenderController.RenderReconstructionAsMat();
            fusion.FusionVolume.Renderer.RenderReconstruction();
            Cv2.WaitKey(0);
            //Export your model in world space (it is transformed already)
            fusion.MeshExporter.ExportVolume(@"scan.ply");
            Console.Read();
        }
    }
}
