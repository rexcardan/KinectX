using System;

namespace KinectXConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ks = new KxStream())
            {
                var color = ks.LatestRGBImage();
                var cvColor = CvColor.FromBGR(color);
                var depth = ks.LatestDepthImage();
                cvColor.Show();
            }
            Console.Read();
        }
    }
}
