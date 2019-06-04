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
using System;

namespace MandelbrotSharp.Imaging
{
    public class RgbaImage
    {
        public RgbaValue[] data;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public RgbaImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            data = new RgbaValue[Width*Height];
        }

        public RgbaImage(RgbaImage image) {
            Width = image.Width;
            Height = image.Height;
            data = new RgbaValue[Width * Height];
            Array.Copy(image.data, data, image.data.Length);
        }

        public void SetPixel(int x, int y, RgbaValue argb)
        {
            data[x + y * Width] = argb;
        }

        public RgbaValue GetPixel(int x, int y)
        {
            return data[x + Width * y];
        }

        public int[] CopyDataAsBits()
        {
            return Array.ConvertAll(data, argb => (int)argb);
        }
    }
}
