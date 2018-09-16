using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Meta
{
    public class KinectSettings
    {
        public const int COLOR_HEIGHT  = 1080;

        public const int COLOR_PIXEL_COUNT  = 2073600;

        public const int COLOR_WIDTH = 1920;

        public const int DEPTH_HEIGHT  = 424;

        public const int DEPTH_PIXEL_COUNT  = 512*424;

        public const int DEPTH_WIDTH  = 512;

        public static int RGBA_STRIDE = 1920 * 4;

        public static int RGB_STRIDE = 1920 * 3;
    }
}
