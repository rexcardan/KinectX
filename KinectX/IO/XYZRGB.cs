using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectX.Extensions;

namespace KinectX.IO
{
    public class XYZRGB
    {
        public static void Export(CameraSpacePoint[] cps, byte[] colorBytes, string outputFileName)
        {
            using (TextWriter writer = new StreamWriter(outputFileName))
            {
                for (int i = 0; i < cps.Length; i++)
                {
                    var v = cps[i];

                    if (v.IsReal())
                    {
                        writer.Write(v.X.ToString("e") + " ");
                        writer.Write(v.Y.ToString("e") + " ");
                        writer.Write(v.Z.ToString("e") + " ");

                        //Assume BGRA format
                        var blue = (int)colorBytes[i * 4];
                        var green = (int)colorBytes[i * 4 + 1];
                        var red = (int)colorBytes[i * 4 + 2];

                        writer.Write(red + " ");
                        writer.Write(green + " ");
                        writer.Write(blue + " ");
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
