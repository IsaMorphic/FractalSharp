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
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class TraditionalAlgorithmProvider<TNumber> : AlgorithmProvider<TNumber> where TNumber : struct
    {
        [Parameter(DefaultValue = 4)]
        public Number<TNumber> EscapeRadius;

        public TraditionalAlgorithmProvider(AlgorithmParams<TNumber> @params) : base(@params)
        {
        }

        public override PointData Run(Complex<TNumber> z0)
        {
            // Initialize some variables..
            Complex<TNumber> z = 0;

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (z.MagnitudeSqu < EscapeRadius && iter < Params.MaxIterations)
            {
                z = z * z + z0;
                iter++;
            }

            return new PointData(z.As<double>(), iter, iter < Params.MaxIterations);
        }
    }
}
