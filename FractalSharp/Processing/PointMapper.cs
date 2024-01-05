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

using FractalSharp.Algorithms;
using System.Numerics;

namespace FractalSharp.Processing
{
    public struct PointMapper<TNumber> where TNumber : struct, INumber<TNumber>
    {
        public Rectangle<TNumber> InputSpace { get; set; }
        public Rectangle<TNumber> OutputSpace { get; set; }

        public TNumber MapPointX(TNumber value)
        {
            return MapValue(value,
                InputSpace.XMin, InputSpace.XMax,
                OutputSpace.XMin, OutputSpace.XMax);
        }

        public TNumber MapPointY(TNumber value)
        {
            return MapValue(value,
                InputSpace.YMin, InputSpace.YMax,
                OutputSpace.YMin, OutputSpace.YMax);
        }

        private static T MapValue<T>(T OldValue, T OldMin, T OldMax, T NewMin, T NewMax) where T : struct, INumber<T>
        {
            T OldRange = OldMax - OldMin;
            T NewRange = NewMax - NewMin;
            T NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
    }
}
