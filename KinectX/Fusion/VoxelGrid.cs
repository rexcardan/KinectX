using OpenCvSharp;
using System;

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
            //the higher 8 bit in this short is distance
            //f(d) = 255 - 256 * d / (2 * T)(where d > 0)
            //- 256 * d / (2 * T)(where d < 0)
            //T is the half width of TSDF truncation band, approximate 10 voxels
            //so when crossing the surface, f(d) will jump from 0 to 255
            //the lower 8 bit in this short is weight, from 0~255
            //distance, weight(d, w) pairs stored at each voxel, and it fall into three categories:
            //uninitialized: d = 128(0x80), w = 0; empty: d = 128; w > 0; in-band: 0 < d < 255, d != 128; w > 0
            var sliceVoxels = new short[XResolution * YResolution];
            for (int x = 0; x < XResolution; x++)
            {
                for (int y = 0; y < YResolution; y++)
                {
                    var pitch = XResolution;
                    var slice = YResolution * pitch;
                    var _3dindex = z * slice + y * pitch + x;
                    var _2dindex = y * pitch + x;
                    var split = BitConverter.GetBytes(Voxels[_3dindex]);
                    var distance = split[1];
                    var weight = split[0];
                    short value = (short)((distance * -1024 / 128.0) + 1024);
                    if (distance < 1) { value = 2000; }
                    sliceVoxels[_2dindex] = value;
                }
            }

            var mat = new Mat(YResolution, XResolution, MatType.CV_16UC1, sliceVoxels);
            return mat;
        }
    }
}
