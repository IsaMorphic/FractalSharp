/*
 *  Copyright 2018-2026 Chosen Few Software
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

using System;

namespace FractalSharp.Imaging
{
    public struct RgbaValue
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
        public byte Alpha { get; }

        public RgbaValue(byte r, byte g, byte b, byte a = 255)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }

        public RgbaValue(string hexColor)
        {
            if (hexColor.Length != 9)
                throw new ArgumentException("Invalid hex color format. Expected format: #RRGGBBAA");

            Alpha = byte.Parse(hexColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            Red = byte.Parse(hexColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            Green = byte.Parse(hexColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            Blue = byte.Parse(hexColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
        }

        private static double lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static explicit operator int(RgbaValue rgba) {
            return (rgba.Alpha << 24) | (rgba.Red << 16) | (rgba.Green << 8) | (rgba.Blue);
        }

        public static RgbaValue LerpColors(RgbaValue a, RgbaValue b, double v) {
            // Linear interpolate red, green, and blue values.
            byte red = (byte)lerp(a.Red, b.Red, v);

            byte green = (byte)lerp(a.Green, b.Green, v);

            byte blue = (byte)lerp(a.Blue, b.Blue, v);

            byte alpha = (byte)lerp(a.Alpha, b.Alpha, v);

            return new RgbaValue(red, green, blue, alpha);
        }
    }
}
