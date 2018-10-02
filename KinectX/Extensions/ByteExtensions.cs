using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectX.Extensions
{
    public static class ByteExtensions
    {
        public static double[,] ToPose(this byte[] poseBytes)
        {
            IFormatter formatter = new BinaryFormatter();
            using (var ms = new MemoryStream(poseBytes))
            {
                var pose = (double[,])formatter.Deserialize(ms);
                return pose;
            }
        }
    }
}
