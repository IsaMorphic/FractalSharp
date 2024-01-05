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

using QuadrupleLib;
using System;
using System.Numerics;

namespace FractalSharp.Numerics.Helpers
{
    public interface INumberConverter<TNumber>
            where TNumber : struct, INumber<TNumber>
    {
        TNumber FromInt32(int x);
        double ToDouble(TNumber x);
    }

    public struct DefaultNumberConverter : INumberConverter<Half>, INumberConverter<float>, INumberConverter<double>, INumberConverter<Float128>
    {
        Half INumberConverter<Half>.FromInt32(int x) => (Half)x;
        double INumberConverter<Half>.ToDouble(Half x) => (double)x;

        float INumberConverter<float>.FromInt32(int x) => x;
        double INumberConverter<float>.ToDouble(float x) => x;

        double INumberConverter<double>.FromInt32(int x) => x;
        double INumberConverter<double>.ToDouble(double x) => x;

        Float128 INumberConverter<Float128>.FromInt32(int x) => x;
        double INumberConverter<Float128>.ToDouble(Float128 x) => (double)x;
    }
}
