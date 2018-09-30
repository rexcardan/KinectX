using KinectX.Extensions;
using System;

namespace KinectX.Mathematics.MatrixDecomp
{
    public class QRDResult
    {
        public double[,] QR { get; set; }
        public double[] Diagonal { get; set; }

        /// <summary>Is the matrix full rank?</summary>
        /// <returns>
        ///     true if R, and hence A, has full rank.
        /// </returns>
        public virtual bool IsFullRank
        {
            get
            {
                for (int j = 0; j < QR.ColumnCount(); j++)
                {
                    if (Diagonal[j] == 0)
                        return false;
                }
                return true;
            }
        }

        public virtual double[,] Solve(double[,] B)
        {
            if (B.RowCount() != QR.RowCount())
            {
                throw new ArgumentException("Row dimensions must agree.");
            }
            if (!IsFullRank)
            {
                throw new SystemException("Matrix is rank deficient.");
            }

            // Copy right hand side
            int nx = B.ColumnCount();
            double[,] X = B.Copy();

            // Compute Y = transpose(Q)*B
            for (int k = 0; k < QR.ColumnCount(); k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    double s = 0.0;
                    for (int i = k; i < QR.RowCount(); i++)
                    {
                        s += QR[i, k]*X[i, j];
                    }
                    s = (-s)/QR[k, k];
                    for (int i = k; i < QR.RowCount(); i++)
                    {
                        X[i, j] += s*QR[i, k];
                    }
                }
            }
            // Solve R*X = Y;
            for (int k = QR.ColumnCount() - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= Diagonal[k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j]*QR[i, k];
                    }
                }
            }

            return (X.Submatrix(0, QR.ColumnCount() - 1, 0, nx - 1));
        }
    }
}