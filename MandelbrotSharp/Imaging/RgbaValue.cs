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
namespace MandelbrotSharp.Imaging
{
    public struct RgbaValue
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public RgbaValue(byte r, byte g, byte b, byte a = 255)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }

        private static double lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static explicit operator int(RgbaValue rgba) {
            return (rgba.Alpha << 24) | (rgba.Red << 16) | (rgba.Green << 8) | (rgba.Blue);
        }

        public static RgbaValue LerpColors(RgbaValue a, RgbaValue b, double alpha) {
            // Initialize final color
            RgbaValue c = new RgbaValue();

            // Linear interpolate red, green, and blue values.
            c.Red = (byte)lerp(a.Red, b.Red, alpha);

            c.Green = (byte)lerp(a.Green, b.Green, alpha);

            c.Blue = (byte)lerp(a.Blue, b.Blue, alpha);

            c.Alpha = (byte)lerp(a.Alpha, b.Alpha, alpha);

            return c;
        }
    }
}
