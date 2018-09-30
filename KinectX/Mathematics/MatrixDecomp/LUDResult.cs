using KinectX.Extensions;
using System;

namespace KinectX.Mathematics.MatrixDecomp
{
    //*Adapted from public domain "General Matrix" lib from NIST and MathWorks, Inc
    public class LudResult
    {
        public double[,] Lu { get; set; }
        public int PivotSign { get; set; }
        public int[] Pivot { get; set; }

        /// <summary>Is the matrix nonsingular?</summary>
        /// <returns>
        ///     true if U, and hence A, is nonsingular.
        /// </returns>
        public virtual bool IsNonSingular
        {
            get
            {
                for (int j = 0; j < Lu.ColumnCount(); j++)
                {
                    if (System.Math.Abs(Lu[j, j]) < 0.01)
                        return false;
                }
                return true;
            }
        }

        public virtual double Determinant()
        {
            if (Lu.RowCount() != Lu.ColumnCount())
            {
                throw new ArgumentException("Matrix must be square.");
            }
            double d = PivotSign;
            for (int j = 0; j < Lu.ColumnCount(); j++)
            {
                d *= Lu[j, j];
            }
            return d;
        }

        /// <summary>Solve A*X = B</summary>
        /// <param name="B">
        ///     A Matrix with as many rows as A and any number of columns.
        /// </param>
        /// <returns>
        ///     X so that L*U*X = B(piv,:)
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     Matrix row dimensions must agree.
        /// </exception>
        /// <exception cref="System.SystemException">
        ///     Matrix is singular.
        /// </exception>
        public virtual double[,] Solve(double[,] B)
        {
            if (B.RowCount() != Lu.RowCount())
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!IsNonSingular)
            {
                throw new SystemException("Matrix is singular.");
            }

            int n = Lu.ColumnCount();
            // Copy right hand side with pivoting
            int nx = B.ColumnCount();
            double[,] X = B.Submatrix(Pivot, 0, nx - 1);

            // Solve L*Y = B(piv,:)
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j]*Lu[i, k];
                    }
                }
            }
            // Solve U*X = Y;
            for (int k = n - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= Lu[k, k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j]*Lu[i, k];
                    }
                }
            }
            return X;
        }
    }
}