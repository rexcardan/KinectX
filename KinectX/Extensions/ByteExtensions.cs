using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

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
