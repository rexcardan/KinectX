using KinectX.Extensions;

namespace KinectX.Mathematics.MatrixDecomp
{
    public class LUD
    {
        //*Adapted from public domain "General Matrix" lib from NIST and MathWorks, Inc
        public static LudResult ComputeLUD(double[,] A)
        {
            var result = new LudResult();
            // Use a "left-looking", dot-product, Crout/Doolittle algorithm.

            result.Lu = A.Copy();
            int m = A.RowCount();
            int n = A.ColumnCount();
            result.Pivot = new int[m];
            for (int i = 0; i < m; i++)
            {
                result.Pivot[i] = i;
            }
            result.PivotSign = 1;
            double[] LUrowi;
            var LUcolj = new double[m];

            // Outer loop.

            for (int j = 0; j < n; j++)
            {
                // Make a copy of the j-th column to localize references.
                for (int i = 0; i < m; i++)
                {
                    LUcolj[i] = result.Lu[i, j];
                }
                // Apply previous transformations.
                for (int i = 0; i < m; i++)
                {
                    LUrowi = result.Lu.GetRow(i);

                    // Most of the time is spent in the following dot product.

                    int kmax = System.Math.Min(i, j);
                    double s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += LUrowi[k]*LUcolj[k];
                    }

                    LUrowi[j] = LUcolj[i] -= s;
                    result.Lu = result.Lu.InsertRow(LUrowi, i);
                }

                // Find pivot and exchange if necessary.

                int p = j;
                for (int i = j + 1; i < m; i++)
                {
                    if (System.Math.Abs(LUcolj[i]) > System.Math.Abs(LUcolj[p]))
                    {
                        p = i;
                    }
                }
                if (p != j)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double t = result.Lu[p, k];
                        result.Lu[p, k] = result.Lu[j, k];
                        result.Lu[j, k] = t;
                    }
                    int k2 = result.Pivot[p];
                    result.Pivot[p] = result.Pivot[j];
                    result.Pivot[j] = k2;
                    result.PivotSign = -result.PivotSign;
                }

                // Compute multipliers.

                if (j < m & result.Lu[j, j] != 0.0)
                {
                    for (int i = j + 1; i < m; i++)
                    {
                        result.Lu[i, j] /= result.Lu[j, j];
                    }
                }
            }
            return result;
        }
    }
}