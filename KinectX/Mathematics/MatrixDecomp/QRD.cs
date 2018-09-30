using KinectX.Extensions;

namespace KinectX.Mathematics.MatrixDecomp
{
    public class QRD
    {
        /// <summary>
        ///     QR Decomposition.*Adapted from public domain "General Matrix" lib from NIST and MathWorks, Inc
        ///     For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
        ///     orthogonal matrix Q and an n-by-n upper triangular matrix R so that
        ///     A = Q*R.
        ///     The QR decompostion always exists, even if the matrix does not have
        ///     full rank, so the constructor will never fail.  The primary use of the
        ///     QR decomposition is in the least squares solution of nonsquare systems
        ///     of simultaneous linear equations.  This will fail if IsFullRank()
        ///     returns false.
        /// </summary>
        public static QRDResult ComputeQRD(double[,] matrix)
        {
            var result = new QRDResult();
            // Initialize.
            result.QR = matrix.Copy();
            int m = matrix.RowCount();
            int n = matrix.ColumnCount();
            result.Diagonal = new double[n];

            // Main loop.
            for (int k = 0; k < n; k++)
            {
                // Compute 2-norm of k-th column without under/overflow.
                double nrm = 0;
                for (int i = k; i < m; i++)
                {
                    nrm = Functions.Hypotenuse(nrm, result.QR[i, k]);
                }

                if (nrm != 0.0)
                {
                    // Form k-th Householder vector.
                    if (result.QR[k, k] < 0)
                    {
                        nrm = -nrm;
                    }
                    for (int i = k; i < m; i++)
                    {
                        result.QR[i, k] /= nrm;
                    }
                    result.QR[k, k] += 1.0;

                    // Apply transformation to remaining columns.
                    for (int j = k + 1; j < n; j++)
                    {
                        double s = 0.0;
                        for (int i = k; i < m; i++)
                        {
                            s += result.QR[i, k]*result.QR[i, j];
                        }
                        s = (-s)/result.QR[k, k];
                        for (int i = k; i < m; i++)
                        {
                            result.QR[i, j] += s*result.QR[i, k];
                        }
                    }
                }
                result.Diagonal[k] = -nrm;
            }
            return result;
        }
    }
}