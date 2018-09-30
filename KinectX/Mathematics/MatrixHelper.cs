
namespace KinectX.Mathematics
{
    public class MatrixHelper
    {
        /// <summary>
        ///     A static identity matrix creator. Returns an identity matrix.
        /// </summary>
        public static double[,] Identity(int dim)
        {
            var values = new double[dim, dim];
            for (int m = 0; m < dim; m++)
            {
                for (int n = 0; n < dim; n++)
                {
                    values[m, n] = m != n ? 0 : 1;
                }
            }
            return values;
        }
    }
}

