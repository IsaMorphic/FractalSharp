/*
 *  Copyright 2018-2020 Chosen Few Software
 *  This file is part of FractalSharp.
 *
 *  FractalSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  FractalSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with FractalSharp.  If not, see <https://www.gnu.org/licenses/>.
 */

using FractalSharp.Numerics.Generic;

namespace FractalSharp.Algorithms.Fractals
{
    public class SquareMandelbrotAlgorithm<TNumber> 
        : IFractalProvider<EscapeTimeParams<TNumber>, TNumber>
        where TNumber : struct
    {
        public static Rectangle<TNumber> GetOutputBounds(EscapeTimeParams<TNumber> @params, Number<TNumber> aspectRatio)
        {
            Number<TNumber> xScale = aspectRatio * Number<TNumber>.Two / @params.Magnification;

            Number<TNumber> xMin = -xScale + @params.Location.Real;
            Number<TNumber> xMax = xScale + @params.Location.Real;

            Number<TNumber> yScale = Number<TNumber>.Two / @params.Magnification;

            Number<TNumber> yMin = yScale + @params.Location.Imag;
            Number<TNumber> yMax = -yScale + @params.Location.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        public static PointData Run(EscapeTimeParams<TNumber> @params, Complex<TNumber> c)
        {
            int iter = 0;
            Complex<TNumber> z = Complex<TNumber>.Zero;

            while (Complex<TNumber>.AbsSqu(z) < @params.EscapeRadius && iter < @params.MaxIterations)
            {
                z = z * z + c;
                iter++;
            }

            return new PointData(z.ToDouble(), iter, iter < @params.MaxIterations ? PointClass.Outer : PointClass.Inner);
        }
    }
}
