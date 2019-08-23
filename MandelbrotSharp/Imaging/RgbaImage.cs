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
        public RgbaValue[] Data;

        public int Width { get; }
        public int Height { get; }

        public RgbaImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            Data = new RgbaValue[Width*Height];
        }

        public RgbaImage(RgbaImage image) {
            Width = image.Width;
            Height = image.Height;
            Data = new RgbaValue[Width * Height];
            Array.Copy(image.Data, Data, Data.Length);
        }

        public void SetPixel(int x, int y, RgbaValue argb)
        {
            Data[x + y * Width] = argb;
        }

        public RgbaValue GetPixel(int x, int y)
        {
            return Data[x + Width * y];
        }

        public int[] CopyDataAsBits()
        {
            return Array.ConvertAll(Data, argb => (int)argb);
        }
    }
}
