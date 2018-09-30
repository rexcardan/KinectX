using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Fusion
{
    public class VoxelGrid
    {
        public int XResolution { get; private set; }
        public int YResolution { get; private set; }
        public int ZResolution { get; private set; }

        public VoxelGrid(int xRes, int yRes, int zRes, short[] voxels)
        {
            XResolution = xRes;
            YResolution = yRes;
            ZResolution = zRes;
            Voxels = voxels;
        }

        public short[] Voxels { get; }

        public Mat GetSlice(int z)
        {
            var sliceVoxels = new byte[XResolution * YResolution];
            for (int x = 0; x < XResolution; x++)
            {
                for (int y = 0; y < YResolution; y++)
                {
                    var pitch = XResolution;
                    var slice = YResolution * pitch;
                    var _3dindex = z * slice + y * pitch + x;
                    var _2dindex = y * pitch + x;
                    var value = Math.Abs((float)Voxels[_3dindex]);
                    var byteValue = (byte)(value / 32768.0 * 255);
                    sliceVoxels[_2dindex] = byteValue;
                }
            }

            var mat = new Mat(YResolution, XResolution, MatType.CV_8UC1, sliceVoxels);
            return mat;
        }
    }
}
