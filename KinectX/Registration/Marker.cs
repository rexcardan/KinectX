using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect.Voxels.Registration
{
    public class Marker
    {
        public int Id { get; set; }
        public Point2f[] Points { get; set; }
        public Point3f[] KinectPositions { get; set; }
    }
}
