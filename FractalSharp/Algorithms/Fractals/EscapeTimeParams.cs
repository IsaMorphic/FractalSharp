/*
 *  Copyright 2018-2024 Chosen Few Software
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
using System;
using System.Numerics;

namespace FractalSharp.Algorithms.Fractals
{
    public struct EscapeTimeParams<TNumber>
        where TNumber : struct, INumber<TNumber>
    {
        public int MaxIterations;
        public Complex<TNumber> Position;
        public TNumber Scale;

        public EscapeTimeParams()
        {
            MaxIterations = 256;
            Position = Complex<TNumber>.Zero;
            Scale = TNumber.One;
        }
    }
}
