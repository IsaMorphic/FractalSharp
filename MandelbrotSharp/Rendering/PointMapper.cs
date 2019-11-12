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
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Rendering
{
    public class PointMapper<TNumberIn, TNumberOut> where TNumberIn : struct where TNumberOut : struct
    {
        public Rectangle<TNumberIn> InputSpace { get; set; }
        public Rectangle<TNumberOut> OutputSpace { get; set; }

        public Number<TNumberOut> MapPointX(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(),
                InputSpace.XMin.As<TNumberOut>(), InputSpace.XMax.As<TNumberOut>(),
                OutputSpace.XMin, OutputSpace.XMax);
        }

        public Number<TNumberOut> MapPointY(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(),
                InputSpace.YMin.As<TNumberOut>(), InputSpace.YMax.As<TNumberOut>(),
                OutputSpace.YMin, OutputSpace.YMax);
        }

        private static Number<TNumber> MapValue<TNumber>(Number<TNumber> OldValue, Number<TNumber> OldMin, Number<TNumber> OldMax, Number<TNumber> NewMin, Number<TNumber> NewMax) where TNumber : struct
        {
            Number<TNumber> OldRange = OldMax - OldMin;
            Number<TNumber> NewRange = NewMax - NewMin;
            Number<TNumber> NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
    }
}
