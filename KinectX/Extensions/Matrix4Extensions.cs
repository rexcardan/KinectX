using Microsoft.Kinect.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectX.Extensions
{
    public static class Matrix4Extensions
    {
        /// <summary>
        ///     Scalar multiplication operator for the Matrix
        /// </summary>
        /// <param name="d">the scalar to multiply each element by</param>
        /// <param name="m2">the matrix of which the elements will be multiplied</param>
        /// <returns>a new matrix3 containing the multiplied values</returns>
        public static Matrix4 MultiplyByScalar(this Matrix4 m1, double scalar)
        {
            double[,] result = m1.Values();

            for (int m = 0; m < 4; m++)
            {
                for (int n = 0; n <4; n++)
                {
                    result[m, n] *= scalar;
                }
            }

            return result.ToMatrix4();
        }


        /// <summary>
        ///     Traditional matrix multiplication operator between to matrices A and B
        /// </summary>
        /// <param name="m1">the first matrix A to get the matrix AB </param>
        /// <param name="m2">the second matrix B to get the matrix AB </param>
        /// <returns>the matrix AB from matrices A and B</returns>
        public static Matrix4 Multiply(this Matrix4 mat1, Matrix4 mat2)
        {
            var m1 = mat1.Values();
            var m2 = mat2.Values();

            double[,] result = new double[4, 4]; //zeroes
            for (int m = 0; m < 4; m++)
            {
                for (int n = 0; n < 4; n++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        result[m, n] += m1[m, k] * m2[k, n];
                    }
                }
            }
            return result.ToMatrix4();
        }

        public static double[,] Values(this Matrix4 m1)
        {
            double[,] values = new double[4, 4];
            values[0, 0] = m1.M11;
            values[0, 1] = m1.M12;
            values[0, 2] = m1.M13;
            values[0, 3] = m1.M14;

            values[1, 0] = m1.M21;
            values[1, 1] = m1.M22;
            values[1, 2] = m1.M23;
            values[1, 3] = m1.M24;

            values[2, 0] = m1.M31;
            values[2, 1] = m1.M32;
            values[2, 2] = m1.M33;
            values[2, 3] = m1.M34;

            values[3, 0] = m1.M41;
            values[3, 1] = m1.M42;
            values[3, 2] = m1.M43;
            values[3, 3] = m1.M44;
            return values;
        }

        public static Matrix4 ToMatrix4(this double[,] values)
        {
            var mat = new Matrix4();
            mat.M11 = (float)values[0, 0];
            mat.M12 = (float)values[0, 1];
            mat.M13 = (float)values[0, 2];
            mat.M14 = (float)values[0, 3];

            mat.M21 = (float)values[1, 0];
            mat.M21 = (float)values[1, 1];
            mat.M21 = (float)values[1, 2];
            mat.M21 = (float)values[1, 3];

            mat.M31 = (float)values[2, 0];
            mat.M31 = (float)values[2, 1];
            mat.M31 = (float)values[2, 2];
            mat.M31 = (float)values[2, 3];

            mat.M41 = (float)values[3, 0];
            mat.M41 = (float)values[3, 1];
            mat.M41 = (float)values[3, 2];
            mat.M41 = (float)values[3, 3];
            return mat;
        }

        public static Matrix4 Scale(this Matrix4 m1, double scale)
        {
            var m2 = Matrix4.Identity.MultiplyByScalar(scale);
            return m2.Multiply(m1);
        }
    }
}
