using KinectX.Data.Listeners;
using KinectX.Fusion.Components;
using KinectX.Fusion.Helpers;
using KinectX.Processors;
using Microsoft.Kinect.Fusion;
using System;
using System.Windows.Controls;

namespace KinectX.Fusion
{
    public class Engine
    {
        public Engine()
        {
            this.ColorProcessor = new FusionColorProcessor();
            this.RenderController = new RenderController();
            this.DepthProcessor = new Processors.FusionDepthProcessor(this);
            this.Resampler = new Resampler(this);
            this.CameraTracker = new CameraTracker(this);
            this.PoseFinder = new PoseFinder(this);
            this.DeltaCalculator = new DeltaCalculator(this);
            this.DataIntegrator = new DataIntegrator(this);
            this.MeshExporter = new MeshExporter(this);
            this.Scanner = new Scanner(this);

            this.FusionVolume = new FusionVolume(this);
            this.PointCloudCalculator = new PointCloudProcessor(this);
        }

        public void InitializeFusionVolume(Matrix4 worldToCameraTx)
        {
            this.FusionVolume = new FusionVolume(this, worldToCameraTx);
            this.PointCloudCalculator = new PointCloudProcessor(this);
        }

        public void SetFusionRenderViewPort(Viewport3D graphicsViewPort)
        {
            this.CubeDrawer = new CubeDrawer(graphicsViewPort, FusionVolume);
        }

        public FusionColorProcessor ColorProcessor { get; private set; }
        public CubeDrawer CubeDrawer { get; set; }

        public T StartFrameListener<T>() where T : FrameListener
        {
            FrameListener = Activator.CreateInstance<T>();
            FrameListener.Initialize();
            return (T)FrameListener;
        }

        public FusionVolume FusionVolume { get; private set; }
        public RenderController RenderController { get; private set; }
        public FrameListener FrameListener { get; set; }
        public Processors.FusionDepthProcessor DepthProcessor { get; private set; }


        public Resampler Resampler { get; private set; }
        public PointCloudProcessor PointCloudCalculator { get; private set; }
        public CameraTracker CameraTracker { get; private set; }
        public PoseFinder PoseFinder { get; private set; }
        public DeltaCalculator DeltaCalculator { get; private set; }
        public DataIntegrator DataIntegrator { get; private set; }
        public MeshExporter MeshExporter { get; private set; }
        public Scanner Scanner { get; private set; }
    }
}
