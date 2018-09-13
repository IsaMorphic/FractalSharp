using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Algorithms
{
    class TraditionalAlgorithmProvider<T> : IAlgorithmProvider<T>
    {
        private IGenericMath<T> TMath;

        private T Zero;
        private T Two;
        private T Four;

        private T offsetX;
        private T offsetY;

        private int MaxIterations;

        public void Init(IGenericMath<T> TMath, T offsetX, T offsetY, int maxIterations)
        {
            this.TMath = TMath;
            MaxIterations = maxIterations;

            this.offsetX = offsetX;
            this.offsetY = offsetY;

            Zero = TMath.fromInt32(0);
            Two = TMath.fromInt32(2);
            Four = TMath.fromInt32(4);
        }

        public PixelData<T> Run(T px, T py)
        {
            T x0 = TMath.Add(px, offsetX);
            T y0 = TMath.Add(py, offsetY);

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
            while (TMath.LessThan(TMath.Add(xx, yy), Four) && iter < MaxIterations)
            {
                // xtemp = xx - yy + x0
                T xtemp = TMath.Add(TMath.Subtract(xx, yy), x0);
                // ytemp = 2 * x * y + y0
                T ytemp = TMath.Add(TMath.Multiply(Two, TMath.Multiply(x, y)), y0);

                if (TMath.EqualTo(x, xtemp) && TMath.EqualTo(y, ytemp))
                {
                    iter = MaxIterations;
                    break;
                }

                x = xtemp;
                y = ytemp;
                xx = TMath.Multiply(x, x);
                yy = TMath.Multiply(y, y);

                iter++;
            }
            return new PixelData<T>(TMath.Add(xx, yy), iter, iter < MaxIterations);
        }

    }
}
