using KinectX.Mathematics;
using KinectX.Mathematics.MatrixDecomp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectX.Extensions
{
    /// <summary>
    ///     Used for working with double[,] as a matrix
    /// </summary>
    public static class Array2DExtensions
    {
        #region SLICING/SUBMATRIX

        public static double[] GetRow(this double[,] matrix, int row)
        {
            int rowSize = matrix.ColumnCount();
            return matrix.Cast<double>().Skip(rowSize * row).Take(rowSize).ToArray();
        }

        public static double[] GetColumn(this double[,] matrix, int col)
        {
            int colSize = matrix.RowCount();
            var result = new double[colSize];
            for (int i = 0; i < colSize; i++)
            {
                result[i] = matrix[i, col];
            }
            return result;
        }

        public static double[,] Submatrix(this double[,] matrix, int[] rowIndices, int startingCol, int endingCol)
        {
            var result = new double[rowIndices.Length, endingCol - startingCol + 1];
            try
            {
                for (int i = 0; i < rowIndices.Length; i++)
                {
                    for (int j = startingCol; j <= endingCol; j++)
                    {
                        result[i, j - startingCol] = matrix[rowIndices[i], j];
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException("Submatrix indices", e);
            }
            return result;
        }

        public static double[,] Submatrix(this double[,] matrix, int startingRow, int endingRow, int startingCol,
            int endingCol)
        {
            var result = new double[endingRow - startingRow + 1, endingCol - startingCol + 1];

            try
            {
                for (int i = startingRow; i <= endingRow; i++)
                {
                    for (int j = startingCol; j <= endingCol; j++)
                    {
                        result[i - startingRow, j - startingCol] = matrix[i, j];
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new IndexOutOfRangeException("Submatrix indices", e);
            }
            return result;
        }

        #endregion

        #region MATRIX PROPERTIES

        public static int RowCount(this double[,] matrix)
        {
            return matrix.GetUpperBound(0) + 1;
        }

        public static int ColumnCount(this double[,] matrix)
        {
            return matrix.GetUpperBound(1) + 1;
        }

        public static bool IsSquare(this double[,] matrix)
        {
            return matrix.RowCount() == matrix.ColumnCount();
        }

        #endregion

        #region MANIPULATION/COPY

        public static double[,] InsertRow(this double[,] matrix, double[] row, int rowPos)
        {
            int rowSize = matrix.ColumnCount();
            if (row.Length != rowSize)
            {
                throw new InvalidSizeException(rowSize, row.Length);
            }
            var rows = new List<double[]>();
            for (int k = 0; k < matrix.RowCount(); k++)
            {
                rows.Add(matrix.GetRow(k));
            }
            if (rows.Count > rowPos)
            {
                rows[rowPos] = row;
            }
            else //Add zeros until position is reached
            {
                while (rows.Count < rowPos)
                {
                    rows.Add(Enumerable.Range(1, rowSize).Select(r => 0.0).ToArray());
                }
                rows.Add(row);
            }
            int m = rows.Count;
            int n = rowSize;
            var result = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = rows[i][j];
                }
            }
            return result;
        }


        public static double[,] Transpose(this double[,] matrix)
        {
            int m = matrix.RowCount();
            int n = matrix.ColumnCount();

            var result = new double[n, m];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }
            return result;
        }

        public static double[,] Copy(this double[,] matrix)
        {
            var copy = new double[matrix.RowCount(), matrix.ColumnCount()];
            Array.Copy(matrix, copy, matrix.Length);
            return copy;
        }

        #endregion

        #region COMPUTATION/SOLVING

        public static SvdResult Svd(this double[,] matrix)
        {
            return Mathematics.MatrixDecomp.Svd.ComputeSvd(matrix);
        }

        public static double[,] Solve(this double[,] matrix, double[,] b)
        {
            return (matrix.IsSquare() ? (LUD.ComputeLUD(matrix)).Solve(b) : (QRD.ComputeQRD(matrix)).Solve(b));
        }

        public static double[,] Inverse(this double[,] matrix)
        {
            return matrix.Solve(MatrixHelper.Identity(matrix.RowCount()));
        }

        public static double Determinant(this double[,] matrix)
        {
            if (matrix.ColumnCount() != matrix.RowCount())
            {
                return double.NaN; //Not a square matrix
            }
            return LUD.ComputeLUD(matrix).Determinant();
        }

        #endregion

        #region OPERATIONS

        public static double[,] ElementWiseOperate(this double[,] matrix1, double[,] matrix2,
            Func<double, double, double> elementWiseOp)
        {
            int m = matrix1.RowCount();
            int n = matrix1.ColumnCount();

            if (matrix1.GetUpperBound(0) != matrix2.GetUpperBound(0))
            {
                throw new InvalidSizeException(matrix1.GetUpperBound(0), matrix2.GetUpperBound(0));
            }
            if (matrix1.GetUpperBound(1) != matrix2.GetUpperBound(1))
            {
                throw new InvalidSizeException(matrix1.GetUpperBound(1), matrix2.GetUpperBound(1));
            }
            var result = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = elementWiseOp(matrix1[i, j], matrix2[i, j]);
                }
            }
            return result;
        }

        public static double[,] ElementWiseOperate(this double[,] matrix1, double scalar,
            Func<double, double, double> elementWiseOp)
        {
            int m = matrix1.GetUpperBound(0) + 1;
            int n = matrix1.GetUpperBound(1) + 1;

            var result = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = elementWiseOp(matrix1[i, j], scalar);
                }
            }
            return result;
        }

        public static double[,] Add(this double[,] matrix1, double[,] matrix2)
        {
            return ElementWiseOperate(matrix1, matrix2, (m1, m2) => { return m1 + m2; });
        }

        public static double[,] Subtract(this double[,] matrix1, double[,] matrix2)
        {
            return ElementWiseOperate(matrix1, matrix2, (m1, m2) => { return m1 - m2; });
        }

        public static double[,] ElementWiseMultiply(this double[,] matrix1, double[,] matrix2)
        {
            return ElementWiseOperate(matrix1, matrix2, (m1, m2) => { return m1 * m2; });
        }

        public static double[,] Multiply(this double[,] matrix1, double[,] matrix2)
        {
            int m1 = matrix1.GetUpperBound(0) + 1;
            int n1 = matrix1.GetUpperBound(1) + 1;
            int m2 = matrix2.GetUpperBound(0) + 1;
            int n2 = matrix2.GetUpperBound(1) + 1;

            if (n1 != m2)
            {
                throw new InvalidSizeException(n1, m2);
            }

            var result = new double[m1, n2];

            for (int m = 0; m < m1; m++)
            {
                for (int n = 0; n < n2; n++)
                {
                    for (int k = 0; k < n1; k++)
                    {
                        result[m, n] += matrix1[m, k] * matrix2[k, n];
                    }
                }
            }

            return result;
        }

        public static double[,] Multiply(this double[,] matrix1, double scalar)
        {
            return ElementWiseOperate(matrix1, scalar, (m1, s) => { return m1 * s; });
        }

        public static double[] Multiply(this double[,] matrix, double[] vector)
        {
            int m = matrix.RowCount();
            int n = matrix.ColumnCount();

            if (vector.Length != n)
            {
                throw new InvalidSizeException(n, vector.Length);
            }

            var result = new List<double>();

            for (int i = 0; i < m; i++)
            {
                double current = 0.0;
                for (int j = 0; j < n; j++)
                {
                    current += matrix[i, j] * vector[j];
                }
                result.Add(current);
            }

            return result.ToArray();
        }


        public static double[,] Divide(this double[,] matrix1, double scalar)
        {
            return ElementWiseOperate(matrix1, scalar, (m1, s) => { return m1 / s; });
        }

        #endregion
    }

    public class InvalidSizeException : Exception
    {
        public InvalidSizeException(int sizeExpected, int sizeActual)
            : base(string.Format("Invalid size. Expected {0} length. Actual was {1}", sizeExpected, sizeActual))
        {
        }
    }
}
