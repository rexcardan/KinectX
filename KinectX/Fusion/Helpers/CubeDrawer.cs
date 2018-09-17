using KinectX.Fusion.Components;
using KinectX.Helpers;
using System;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace KinectX.Fusion.Helpers
{
    public class CubeDrawer
    {
        private Viewport3D _gvp;

        /// <summary>
        /// The volume cube 3D graphical representation
        /// </summary>
        private ScreenSpaceLines3D volumeCube;

        /// <summary>
        /// The volume cube 3D graphical representation
        /// </summary>
        private ScreenSpaceLines3D volumeCubeAxisX;

        /// <summary>
        /// The volume cube 3D graphical representation
        /// </summary>
        private ScreenSpaceLines3D volumeCubeAxisY;

        /// <summary>
        /// The volume cube 3D graphical representation
        /// </summary>
        private ScreenSpaceLines3D volumeCubeAxisZ;

        /// <summary>
        /// The axis-aligned coordinate cross X axis
        /// </summary>
        private ScreenSpaceLines3D axisX;

        /// <summary>
        /// The axis-aligned coordinate cross Y axis
        /// </summary>
        private ScreenSpaceLines3D axisY;

        /// <summary>
        /// The axis-aligned coordinate cross Z axis
        /// </summary>
        private ScreenSpaceLines3D axisZ;

        /// <summary>
        /// Volume Cube 3D graphics line color
        /// </summary>
        private static System.Windows.Media.Color volumeCubeLineColor = System.Windows.Media.Color.FromArgb(200, 0, 200, 0);   // Green, partly transparent

        /// <summary>
        /// Volume Cube and WPF3D Origin coordinate cross axis 3D graphics line thickness in screen pixels
        /// </summary>
        private const int LineThickness = 2;
        private FusionVolume vol;

        public bool HaveAddedVolumeCube { get; private set; }

        public CubeDrawer(Viewport3D gvp, FusionVolume fv)
        {
            vol = fv;
            _gvp = gvp;
            this.CreateCube3DGraphics(volumeCubeLineColor, LineThickness, new Vector3D(0, 0, 0));
        }
        /// <summary>
        /// Remove the volume cube and axes from the visual tree
        /// </summary>
        private void RemoveVolumeCube3DGraphics()
        {
            if (null != this.volumeCube)
            {
                _gvp.Children.Remove(this.volumeCube);
            }

            if (null != this.volumeCubeAxisX)
            {
                _gvp.Children.Remove(this.volumeCubeAxisX);
            }

            if (null != this.volumeCubeAxisY)
            {
                _gvp.Children.Remove(this.volumeCubeAxisY);
            }

            if (null != this.volumeCubeAxisZ)
            {
                _gvp.Children.Remove(this.volumeCubeAxisZ);
            }

            this.HaveAddedVolumeCube = false;
        }

        /// <summary>
        /// Create an axis-aligned volume cube for rendering.
        /// </summary>
        /// <param name="color">The color of the volume cube.</param>
        /// <param name="thickness">The thickness of the lines in screen pixels.</param>
        /// <param name="translation">World to volume translation vector.</param>
        private void CreateCube3DGraphics(System.Windows.Media.Color color, int thickness, Vector3D translation)
        {
            // Scaler for cube size
            float cubeSizeScaler = 1.0f;

            // Before we created a volume which contains the head
            // Here we create a graphical representation of this volume cube
            float oneOverVpm = 1.0f / FusionVolume.VoxelsPerMeter;

            // This cube is world axis aligned
            float cubeSideX = FusionVolume.VoxelsX * oneOverVpm * cubeSizeScaler;
            float halfSideX = cubeSideX * 0.5f;

            float cubeSideY = FusionVolume.VoxelsY * oneOverVpm * cubeSizeScaler;
            float halfSideY = cubeSideY * 0.5f;

            float cubeSideZ = FusionVolume.VoxelsZ * oneOverVpm * cubeSizeScaler;
            float halfSideZ = cubeSideZ * 0.5f;

            // The translation vector is from the origin to the volume front face
            // And here we describe the translation Z as from the origin to the cube center
            // So we continue to translate half volume size align Z
            translation.Z -= halfSideZ / cubeSizeScaler;

            this.volumeCube = new ScreenSpaceLines3D();
            this.volumeCube.Points = new Point3DCollection();

            // Front face
            // TL front - TR front
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));

            // TR front - BR front
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));

            // BR front - BL front
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));

            // BL front - TL front
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));

            // Rear face
            // TL rear - TR rear
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));

            // TR rear - BR rear
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));

            // BR rear - BL rear
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));

            // BL rear - TL rear
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));

            // Connecting lines
            // TL front - TL rear
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));

            // TR front - TR rear
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));

            // BR front - BR rear
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));

            // BL front - BL rear
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, -halfSideZ + translation.Z));
            this.volumeCube.Points.Add(new Point3D(-halfSideX + translation.X, -halfSideY + translation.Y, halfSideZ + translation.Z));

            this.volumeCube.Thickness = thickness;
            this.volumeCube.Color = color;

            this.volumeCubeAxisX = new ScreenSpaceLines3D();

            this.volumeCubeAxisX.Points = new Point3DCollection();
            this.volumeCubeAxisX.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCubeAxisX.Points.Add(new Point3D(-halfSideX + 0.1f + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));

            this.volumeCubeAxisX.Thickness = thickness + 2;
            this.volumeCubeAxisX.Color = System.Windows.Media.Color.FromArgb(200, 255, 0, 0);   // Red (X)

            this.volumeCubeAxisY = new ScreenSpaceLines3D();

            this.volumeCubeAxisY.Points = new Point3DCollection();
            this.volumeCubeAxisY.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCubeAxisY.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY - 0.1f + translation.Y, halfSideZ + translation.Z));

            this.volumeCubeAxisY.Thickness = thickness + 2;
            this.volumeCubeAxisY.Color = System.Windows.Media.Color.FromArgb(200, 0, 255, 0);   // Green (Y)

            this.volumeCubeAxisZ = new ScreenSpaceLines3D();

            this.volumeCubeAxisZ.Points = new Point3DCollection();
            this.volumeCubeAxisZ.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ + translation.Z));
            this.volumeCubeAxisZ.Points.Add(new Point3D(-halfSideX + translation.X, halfSideY + translation.Y, halfSideZ - 0.1f + translation.Z));

            this.volumeCubeAxisZ.Thickness = thickness + 2;
            this.volumeCubeAxisZ.Color = System.Windows.Media.Color.FromArgb(200, 0, 0, 255);   // Blue (Z)
        }

        public void UpdateVolumeCube()
        {
            // Update the graphics volume cube rendering
            _gvp.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                        // re-create the volume cube display with the new size
                    this.RemoveVolumeCube3DGraphics();
                    this.DisposeVolumeCube3DGraphics();
                    this.CreateCube3DGraphics(volumeCubeLineColor, LineThickness, new Vector3D(0, 0, 0));
                    this.AddVolumeCube3DGraphics();
                }));

        }

        /// <summary>
        /// Dispose the volume cube and axes
        /// </summary>
        private void DisposeVolumeCube3DGraphics()
        {
            if (HaveAddedVolumeCube)
            {
                this.RemoveVolumeCube3DGraphics();
            }

            if (null != this.volumeCube)
            {
                this.volumeCube.Dispose();
                this.volumeCube = null;
            }

            if (null != this.volumeCubeAxisX)
            {
                this.volumeCubeAxisX.Dispose();
                this.volumeCubeAxisX = null;
            }

            if (null != this.volumeCubeAxisY)
            {
                this.volumeCubeAxisY.Dispose();
                this.volumeCubeAxisY = null;
            }

            if (null != this.volumeCubeAxisZ)
            {
                this.volumeCubeAxisZ.Dispose();
                this.volumeCubeAxisZ = null;
            }
        }

        /// <summary>
        /// Add the volume cube and axes to the visual tree
        /// </summary>
        private void AddVolumeCube3DGraphics()
        {
            if (HaveAddedVolumeCube)
            {
                return;
            }

            if (null != this.volumeCube)
            {
                _gvp.Children.Add(this.volumeCube);

                HaveAddedVolumeCube = true;
            }

            if (null != this.volumeCubeAxisX)
            {
                _gvp.Children.Add(this.volumeCubeAxisX);
            }

            if (null != this.volumeCubeAxisY)
            {
                _gvp.Children.Add(this.volumeCubeAxisY);
            }

            if (null != this.volumeCubeAxisZ)
            {
                _gvp.Children.Add(this.volumeCubeAxisZ);
            }
        }
    }
}
