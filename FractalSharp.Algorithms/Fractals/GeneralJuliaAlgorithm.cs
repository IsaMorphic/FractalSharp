/*
 *  Copyright 2018-2026 Chosen Few Software
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
using FractalSharp.Numerics.Helpers;
using System.Numerics;

namespace FractalSharp.Algorithms.Fractals
{
    public record struct GeneralJuliaAlgorithmParams<TNumber> :
        IFractalProviderParams<TNumber> where TNumber :
        unmanaged, IFloatingPointIeee754<TNumber>
    {
        public int MaxIterations { get; set; }

        public Complex<TNumber> Position { get; set; }

        public TNumber Scale { get; set; }

        public Complex<TNumber> Constant { get; set; }

        public Complex<TNumber> Power { get; set; }

        public GeneralJuliaAlgorithmParams()
        {
            MaxIterations = 256;
            Position = Complex<TNumber>.Zero;
            Scale = TNumber.One;
            Constant = new(TNumber.CreateSaturating(0.3), TNumber.CreateSaturating(-0.01));
            Power = new(TNumber.CreateSaturating(2.0), TNumber.Zero);
        }
    }

    public class GeneralJuliaAlgorithm<TNumber, TConverter> :
        IFractalProvider<GeneralJuliaAlgorithmParams<TNumber>, TNumber>
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>
        where TConverter : struct, INumberConverter<TNumber>
    {
        private static readonly TNumber _two = TNumber.One + TNumber.One;

        public static Rectangle<TNumber> GetOutputBounds(GeneralJuliaAlgorithmParams<TNumber> @params, TNumber aspectRatio)
        {
            TNumber xScale = aspectRatio * _two / @params.Scale;

            TNumber xMin = -xScale + @params.Position.Real;
            TNumber xMax = xScale + @params.Position.Real;

            TNumber yScale = _two / @params.Scale;

            TNumber yMin = yScale + @params.Position.Imag;
            TNumber yMax = -yScale + @params.Position.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        public static PointData<double> Run(GeneralJuliaAlgorithmParams<TNumber> @params, Complex<TNumber> z)
        {
            int iter = 0;
            for (; iter < @params.MaxIterations; iter++)
            {
                if (Complex<TNumber>.AbsSqu(z) > _two * _two) break;
                z = Complex<TNumber>.Pow(z, @params.Power) + @params.Constant;
            }

            TConverter floatConverter = default;
            var doubleReal = floatConverter.ToDouble(z.Real);
            var doubleImag = floatConverter.ToDouble(z.Imag);

            return new PointData<double>(new Complex<double>(doubleReal, doubleImag), iter,
                iter < @params.MaxIterations ? PointClass.Outer : PointClass.Inner);
        }
    }
}
