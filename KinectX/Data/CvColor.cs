﻿using KinectX.Meta;
using OpenCvSharp;
using System;
using System.Runtime.InteropServices;

namespace KinectX.Data
{
    public class CvColor : Mat
    {
        public CvColor(byte[] yuy2Pixels) : base(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC4)
        {
            using (var yuy2 = new Mat(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, MatType.CV_8UC2, yuy2Pixels))
            {
                Cv2.CvtColor(yuy2, this, ColorConversionCodes.YUV2BGRA_YUY2);
            }
        }

        public static CvColor FromBGR(byte[] color)
        {
            return new CvColor(MatType.CV_8UC4, color);
        }

        public byte[] GetBRGABytes()
        {
            byte[] data = new byte[KinectSettings.COLOR_HEIGHT * KinectSettings.COLOR_WIDTH * 4];
            Marshal.Copy(this.Data, data, 0, data.Length);
            return data;
        }

        private CvColor(MatType type, Array data) : base(KinectSettings.COLOR_HEIGHT, KinectSettings.COLOR_WIDTH, type, data) { }
    }
}
