using KinectX.Extensions;

namespace KinectX.Mathematics.MatrixDecomp
{
    public class SvdResult
    {
        public SvdResult(double[,] u, double[] w, double[,] v)
        {
            U = u;
            W = new double[w.Length, w.Length];
            for (int i = 0; i < w.Length; i++)
            {
                W[i, i] = w[i];
            }
            Vt = v.Transpose();
        }

        public double[,] U { get; set; }
        public double[,] W { get; set; }
        public double[,] Vt { get; set; }
    }
}