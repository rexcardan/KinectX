using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Registration
{
    public class Calibrator
    {
        public static double[,] Calibrate(byte[] colors, ushort[] depth)
        {
            //Define Board
            var cube = CoordinateDefinition.Cube();
            //Look for Board
            //Calculate Camera Pose

            throw new NotImplementedException();
        }
    }
}
