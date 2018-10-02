using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.IO
{
    public class PoseFormatter
    {
        public static byte[] PoseToBytes(double[,] pose)
        {
            IFormatter formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, pose);
                return ms.ToArray();
            }
        }

        public static double[,] BytesToPose(byte[] poseBytes)
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
