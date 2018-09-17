using KinectX.Fusion.Helpers;
using NLog;
using System;
using System.IO;

namespace KinectX.Fusion.Components
{
    public class MeshExporter
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        private Engine engine;

        public MeshExporter(Engine engine)
        {
            this.engine = engine;
        }

        public bool IsSavingMesh { get; internal set; }

        public void ExportVolume(string path)
        {
            var mesh = engine.FusionVolume.Reconstruction.CalculateMesh(1);

            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    _logger.Info($"Exporting mesh to {path}");
                    // Default to flip Y,Z coordinates on save
                    KinectFusionHelper.SaveAsciiPlyMesh(mesh, writer, false, true);
                }
                _logger.Info($"Saved mesh successfully to {path}");
            }
            catch (Exception e)
            {
                _logger.Error($"Problem saving mesh to {path}");
            }
        }
    }
}
