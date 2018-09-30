﻿using KinectX.Data;
using KinectX.Data.Listeners;
using KinectX.Extensions;
using KinectX.Fusion;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.IO;
using KinectX.Network;
using KinectX.Registration;
using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectXConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a defined registration pattern - in this case a cube
            var cube = CoordinateDefinition.Cube();

            //Find registration
            var xef = new Xef(@"../../../Resources/cube.xef");
            var colorCv = xef.LoadCvColorFrame(0);

            //Find and draw (make sure it can be found)
            var markers = Vision.FindAruco(colorCv);
            //Vision.DrawAruco(colorCv).Show();

            //Calculate pose
            var _3dImage = xef.LoadCameraSpace(5);
            var kxTransform = Vision.GetPoseFromImage(cube, _3dImage, markers);
            var pose = kxTransform.FusionCameraPose.ToMatrix4();
            var fusion = new Engine();
            FusionVolume.VoxelsPerMeter = 128;
            FusionVolume.VoxelsX = 384;
            FusionVolume.VoxelsY = 384;
            FusionVolume.VoxelsZ = 384;
            //Offset volume away from Kinect center (so you actually get some data)
            fusion.InitializeFusionVolume(pose);
            VolumeResetter.TranslateResetPoseByMinDepthThreshold = false;

            fusion.DataIntegrator.CaptureColor = false;
            var listener = fusion.StartFrameListener<XefFrameListener>();
            listener.SetXefFile(@"../../../Resources/cube.xef");

            //fusion.FusionVolume.ResetReconstruction(kxTransform.FusionCameraPose.ToMatrix4());
            fusion.Scanner.Scan(3, false);

            fusion.RenderController.RenderReconstructionAsMat();
            fusion.FusionVolume.Renderer.RenderReconstruction();
            var voxelGrid = fusion.FusionVolume.GetVoxels();
            var initialized = voxelGrid.Voxels.Count(v => v != short.MinValue);
            var slice = voxelGrid.GetSlice(384/2);
            
            slice.Show();
           // fusion.MeshExporter.ExportVolume(@"cube.ply");
            Console.Read();
        }
    }
}
