using MandelbrotSharp.Imaging;
using MiscUtil;
using System;
using System.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class TraditionalAlgorithmProvider<T> : AlgorithmProvider<T>
    {
        private T Zero = Operator.Convert<int, T>(0);
        private T Two = Operator.Convert<int, T>(2);
        private T Four = Operator.Convert<int, T>(4);

        [Parameter(DefaultValue = 4)]
        public T BailoutValue;

        public override PixelData Run(T px, T py)
        {
            T x0 = px;
            T y0 = py;

            // Initialize some variables..
            T x = Zero;
            T y = Zero;

            // Define x squared and y squared as their own variables
            // To avoid unnecisarry multiplication.
            T xx = Zero;
            T yy = Zero;

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (Operator.LessThan(Operator.Add(xx, yy), BailoutValue) && iter < Params.MaxIterations)
            {
                // xtemp = xx - yy + x0
                T xtemp = Operator.Add(Operator.Subtract(xx, yy), x0);
                // ytemp = 2 * x * y + y0
                T ytemp = Operator.Add(Operator.Multiply(Two, Operator.Multiply(x, y)), y0);

                if (Operator.Equal(x, xtemp) && Operator.Equal(y, ytemp))
                {
                    iter = Params.MaxIterations;
                    break;
                }

                x = xtemp;
                y = ytemp;
                xx = Operator.Multiply(x, x);
                yy = Operator.Multiply(y, y);

                iter++;
            }
            return new PixelData(new Complex(Operator.Convert<T, double>(x), Operator.Convert<T, double>(y)), iter, iter >= Params.MaxIterations);
        }
    }
}
