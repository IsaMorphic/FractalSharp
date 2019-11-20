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
using System.Threading;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms.Fractals
{
    public abstract class EscapeTimeParams<TNumber> 
        : FractalParams<TNumber>
        where TNumber : struct
    {
        public Number<TNumber> EscapeRadius { get; set; }
    }

    public abstract class EscapeTimeAlgorithm<TNumber, TParam>
        : FractalProvider<TNumber, TParam>,
          IFractalProvider<TNumber>
        where TParam : EscapeTimeParams<TNumber>
        where TNumber : struct
    {
        protected override bool Initialize(CancellationToken cancellationToken) => true;

        public override Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio)
        {
            Number<TNumber> xScale = aspectRatio * Number<TNumber>.From(2.0) / Params.Magnification;

            Number<TNumber> xMin = -xScale + Params.Location.Real;
            Number<TNumber> xMax = xScale + Params.Location.Real;

            Number<TNumber> yScale = Number<TNumber>.From(2.0) / Params.Magnification;

            Number<TNumber> yMin = yScale + Params.Location.Imag;
            Number<TNumber> yMax = -yScale + Params.Location.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        public override PointData Run(Complex<TNumber> mappedPoint)
        {
            // Initialize some variables..
            Complex<TNumber> prevOutput = GetInitialValue(mappedPoint);

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (Complex<TNumber>.AbsSqu(prevOutput) < Params.EscapeRadius && iter < Params.MaxIterations)
            {
                prevOutput = DoIteration(prevOutput, mappedPoint);
                iter++;
            }

            return new PointData(prevOutput.As<double>(), iter, iter < Params.MaxIterations);
        }

        protected abstract Complex<TNumber> DoIteration(Complex<TNumber> prevOutput, Complex<TNumber> mappedPoint);

        protected virtual Complex<TNumber> GetInitialValue(Complex<TNumber> mappedPoint) => Complex<TNumber>.Zero;
    }
}
