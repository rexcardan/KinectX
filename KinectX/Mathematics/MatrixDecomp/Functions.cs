using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = System.Math;

namespace KinectX.Mathematics.MatrixDecomp
{
    public class Functions
    {
        public static double Hypotenuse(double a, double b)
        {
            double r;
            if (M.Abs(a) > M.Abs(b))
            {
                r = b / a;
                r = M.Abs(a) * M.Sqrt(1 + r * r);
            }
            else if (b != 0)
            {
                r = a / b;
                r = M.Abs(b) * M.Sqrt(1 + r * r);
            }
            else
            {
                r = 0.0;
            }
            return r;
        }
    }
}
