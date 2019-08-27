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
using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Data
{
    public struct Rectangle<TNumber> where TNumber : struct
    {
        public Number<TNumber> XMin { get; }
        public Number<TNumber> XMax { get; }
        public Number<TNumber> YMin { get; }
        public Number<TNumber> YMax { get; }

        public Rectangle(Number<TNumber> xMin, Number<TNumber> xMax, Number<TNumber> yMin, Number<TNumber> yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }
    }
}
