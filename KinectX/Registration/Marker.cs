using OpenCvSharp;

namespace KinectX.Registration
{
    public class Marker
    {
        /// <summary>
        /// The Id of the ARUCO marker
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The corners of the ARUCO marker
        /// </summary>
        public Point2f[] Points { get; set; }
        /// <summary>
        /// The 3D position of the marker in Kinect (Kx) space
        /// </summary>
        public Point3f KxCenter { get; internal set; }
        public double Area { get; internal set; }
        public Scalar MaskSum { get; internal set; }
    }
}
