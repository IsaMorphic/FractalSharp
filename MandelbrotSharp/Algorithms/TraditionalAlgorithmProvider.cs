using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MiscUtil;
using System;
using System.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class TraditionalAlgorithmProvider<T> : AlgorithmProvider<T> where T : struct
    {
        [Parameter(DefaultValue = 4)]
        public Number<T> BailoutValue;

        public override PixelData Run(Number<T> px, Number<T> py)
        {
            Number<T> x0 = px;
            Number<T> y0 = py;

            // Initialize some variables..
            Number<T> x = 0;
            Number<T> y = 0;

            // Define x squared and y squared as their own variables
            // To avoid unnecisarry multiplication.
            Number<T> xx = 0;
            Number<T> yy = 0;

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (xx + yy < BailoutValue && iter < Params.MaxIterations)
            {
                // xtemp = xx - yy + x0
                Number<T> xtemp = xx - yy + x0;
                // ytemp = 2 * x * y + y0
                Number<T> ytemp = 2 * x * y + y0;

                if (x == xtemp && y == ytemp)
                {
                    iter = Params.MaxIterations;
                    break;
                }

                x = xtemp;
                y = ytemp;
                xx = x * x;
                yy = y * y;

                iter++;
            }
            return new PixelData(new Complex(x.As<double>(), y.As<double>()), iter, iter >= Params.MaxIterations);
        }
    }
}
