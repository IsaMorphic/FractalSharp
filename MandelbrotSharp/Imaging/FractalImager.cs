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
using MandelbrotSharp.Processing;

namespace MandelbrotSharp.Imaging
{
    public abstract class FractalImager
    {
        public void CreateImage(PointData[,] data, PointColorer colorer, Gradient outerColors, RgbaValue innerColor)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            InitializeImage(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    PointData p = data[y, x];
                    if (p.Escaped)
                    {
                        double index = colorer.GetIndexFromPointData(p);
                        WritePixel(x, y, outerColors[index]);
                    }
                    else
                        WritePixel(x, y, innerColor);
                }
            }
        }
        public abstract void InitializeImage(int width, int height);
        public abstract void WritePixel(int x, int y, RgbaValue color);
    }
}
