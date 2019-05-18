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
using MandelbrotSharp.Utilities;

namespace MandelbrotSharp.Imaging
{
    public struct RgbaValue
    {
        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        public RgbaValue(byte r, byte g, byte b, byte a = 255)
        {
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }

        public static explicit operator int(RgbaValue rgba) {
            return (rgba.alpha << 24) | (rgba.red << 16) | (rgba.green << 8) | (rgba.blue);
        }

        public static RgbaValue LerpColors(RgbaValue a, RgbaValue b, double alpha) {
            // Initialize final color
            RgbaValue c = new RgbaValue();

            // Linear interpolate red, green, and blue values.
            c.red = (byte)Utils.lerp(a.red, b.red, alpha);

            c.green = (byte)Utils.lerp(a.green, b.green, alpha);

            c.blue = (byte)Utils.lerp(a.blue, b.blue, alpha);

            c.alpha = (byte)Utils.lerp(a.alpha, b.alpha, alpha);

            return c;
        }
    }
}
