/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using System.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class TraditionalAlgorithmProvider<T> : AlgorithmProvider<T> where T : struct
    {
        [Parameter(DefaultValue = 4)]
        public Number<T> BailoutValue;

        protected override PixelData Run(Number<T> x0, Number<T> y0)
        {
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
                Number<T> xtemp = xx - yy + x0;
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
            return new PixelData(new Complex((double)x, (double)y), iter, iter >= Params.MaxIterations);
        }
    }
}
