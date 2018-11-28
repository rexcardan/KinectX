using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectX.Extensions;
using OpenCvSharp;

namespace KinectX.IO
{
    public class XYZRGB
    {
        public XYZRGB(double x, double y, double z, int r, int g, int b)
        {
            this.CameraSpacePoint = new CameraSpacePoint() { X = (float)x, Y = (float)y, Z = (float)z };
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public CameraSpacePoint CameraSpacePoint { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

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

        public static void Export(XYZRGB[] xyzs, string outputFileName)
        {
            using (TextWriter writer = new StreamWriter(outputFileName))
            {
                for (int i = 0; i < xyzs.Length; i++)
                {
                    var v = xyzs[i].CameraSpacePoint;

                    if (v.IsReal())
                    {
                        writer.Write(v.X.ToString("e") + " ");
                        writer.Write(v.Y.ToString("e") + " ");
                        writer.Write(v.Z.ToString("e") + " ");

                        //Assume BGRA format
                        var blue = xyzs[i].B;
                        var green = xyzs[i].G;
                        var red = xyzs[i].R;

                        writer.Write(red + " ");
                        writer.Write(green + " ");
                        writer.Write(blue + " ");
                        writer.WriteLine();
                    }
                }
            }
        }

        public static XYZRGB[] Import(string path)
        {
            List<XYZRGB> cps = new List<XYZRGB>();
            var lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var split = line.Split(' ');
                try
                {
                    var x = float.Parse(split[0]);
                    var y = float.Parse(split[1]);
                    var z = float.Parse(split[2]);
                    var r = int.Parse(split[3]);
                    var g = int.Parse(split[4]);
                    var b = int.Parse(split[5]);

                    cps.Add(new XYZRGB(x, y, z, r, g, b));
                }
                catch (Exception e)
                {
                    throw new Exception($"File couldn't be parsed. Check line {i} : {line}");
                }
            }
            return cps.ToArray();
        }
        

        public static void Export(IEnumerable<Point3f> points, Scalar bgrColor, string outputFileName)
        {
            using (TextWriter writer = new StreamWriter(outputFileName))
            {
                foreach (var v in points)
                {

                    writer.Write(v.X.ToString("e") + " ");
                    writer.Write(v.Y.ToString("e") + " ");
                    writer.Write(v.Z.ToString("e") + " ");

                    //Assume BGRA format
                    var blue = (int)bgrColor.Val0;
                    var green = (int)bgrColor.Val1;
                    var red = (int)bgrColor.Val2;

                    writer.Write(red + " ");
                    writer.Write(green + " ");
                    writer.Write(blue + " ");
                    writer.WriteLine();

                }
            }
        }

        public static void Export(Mat matPoints, Scalar scalar, string outputFileName)
        {
            var points = matPoints.AsPoint3fs();
            Export(points, scalar, outputFileName);
        }
    }
}
