/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms.Fractals
{
    public class GlitchMandelbrotAlgorithm<TNumber> :
        EscapeTimeAlgorithm<TNumber, EscapeTimeParams<TNumber>>
        where TNumber : struct
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z, Complex<TNumber> c)
        {
            Number<TNumber> real = z.Real;
            Number<TNumber> imag = z.Imag;
            real = real * real - imag * imag + c.Real;
            imag = Number<TNumber>.Two * real * imag + c.Imag;
            return new Complex<TNumber>(real, imag);
        }
    }
}
