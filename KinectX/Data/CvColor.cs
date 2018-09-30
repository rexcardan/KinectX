using KinectX.Meta;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Data
{
    public class CvColor : Mat
    {
        public CvColor(byte[] yuy2Pixels) : base(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC4)
        {
            using (var yuy2 = new Mat(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC2, yuy2Pixels))
            {
                Cv2.CvtColor(yuy2, this, ColorConversionCodes.YUV2RGBA_YUY2);
            }
        }
    }
}
