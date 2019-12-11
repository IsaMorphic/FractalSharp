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

namespace MandelbrotSharp.Algorithms
{
    public enum PointClass
    {
        Inner = 0,
        Outer = 1
    }

    public struct PointData
    {
        public Complex<double> ZValue { get; }
        public int IterCount { get; }
        public PointClass PointClass { get; }

        public PointData(Complex<double> zValue, int iterCount, PointClass pointClass)
        {
            ZValue = zValue;
            IterCount = iterCount;
            PointClass = pointClass;
        }
    }
}
