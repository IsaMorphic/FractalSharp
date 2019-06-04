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
using System.Numerics;

namespace MandelbrotSharp.Imaging
{
    public class PixelData
    {
        public Complex ZValue { get; private set; }
        public int IterCount { get; private set; }
        public bool Escaped { get; private set; }

        public PixelData(Complex ZValue, int IterCount, bool Escaped)
        {
            this.ZValue = ZValue;
            this.IterCount = IterCount;
            this.Escaped = Escaped;
        }
    }
}
