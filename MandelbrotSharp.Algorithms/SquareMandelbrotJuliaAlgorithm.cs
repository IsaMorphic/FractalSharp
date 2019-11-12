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
    public class SquareMandelbrotJuliaParams<TNumber> : EscapeTimeParams<TNumber> where TNumber : struct
    {
        public Complex<TNumber> Coordinates { get; set; }

        public override IAlgorithmParams Copy()
        {
            return new SquareMandelbrotJuliaParams<TNumber>
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,
                EscapeRadius = EscapeRadius,

                Coordinates = Coordinates
            };
        }
    }

    public class SquareMandelbrotJuliaAlgorithm<TNumber> :
        JuliaAlgorithm<TNumber, SquareMandelbrotJuliaParams<TNumber>>
        where TNumber : struct
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z)
        {
            return z * z + Params.Coordinates;
        }
    }
}
