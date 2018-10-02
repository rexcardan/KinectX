# KinectX
Kinect v2 Extension library

## Introduction
The Kinect v2 was such a great piece of hardware but one of its major drawbacks was that it wasn't easy
to develop for. The KinectX library is designed to make Kinect development much easier by
abstracting concepts and creating a proper library for the things most people want. Some of the 
highlight features are: 

* Easier Kinect access
  * Get the color frame in 2 lines of code 
* OpenCV Integration
  * Depth frame and color frame in Cv2.Mat format for computer vision applications
  * ARUCO pattern recognition and pose estimation built in and easy
* Easier more straightfoward Kinect Fusion
  * Kinect Fusion code is classified into proper classes with smaller more understandable parts
* Multikinect setup
  * A Kinect service can be installed on remote computers to allow remote access to :
    * Remote color frame
    * Remote depth frame
    * Remote audio
    * Remote XEF recording and retrieving
* Easier XEF file handling
  * Fine control over depth and color frame playback
  * Super easy XEF recording
* NLog integration
* Matrix math
  * SVD, LUT, Inverse and Transpose built in for quick Matrix math
* Rendering pipelines
  * Render fusion images
  * Render color/depth frame

## Examples

### KxStream easy access to data
```cs
            using (var ks = new KxStream())
            {
                var color = ks.LatestRGBImage();
                var cvColor = CvColor.FromBGR(color);
                var depth = ks.LatestDepthImage();
                cvColor.Show();
            }
            Console.Read();
```
### XEF with OpenCV
```cs
            var xef = new Xef(@"../../../Resources/cube.xef");
            //Load the first depth frame (frame 0)
            var depth = xef.LoadDepthFrame(0);
            //Load the first color frame (frame 0)
            var color = xef.LoadColorFrame(0);
            var cvColor = new CvColor(color);
            var cvDepth = new CvDepth(depth);
            //render kinect color to UI (using KinectX.Extensions;)
            cvColor.Show();
```
### Kinect Fusion in 12 lines of code!
```cs
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
```
### Registered Kinect Fusion (Camera pose determiniation in REAL WORLD space!)
```cs
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
```
### Remote server example
```cs
            KinectX.Network.KxServer.Start();
            Console.ReadLine();
```

### Client to remote server
```cs
            var cameras = KxServerFinder.FindServers();
            foreach (var cam in cameras)
            {
                var depth = cam.LatestDepthImage();
                //Do something
            }
```
### Client record XEF remotely and retrieve
```cs
            var servers = KxServerFinder.FindServers();
            foreach (var s in servers)
            {
                var name = s.Endpoint.Address.Uri.DnsSafeHost;
                var success = s.RecordXef(TimeSpan.FromMilliseconds(1000));
                var xef = s.LastRecording();
                //Store xef file locally
                File.WriteAllBytes($"{name}.xef", xef);
            }
```
