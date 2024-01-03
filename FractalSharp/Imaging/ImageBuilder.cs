/*
 *  Copyright 2018-2020 Chosen Few Software
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
using System.Threading.Tasks;

namespace FractalSharp.Imaging
{
    public abstract class ImageBuilder
    {
        public void CreateImage(double[,] outerIndicies, double[,] innerIndicies, Gradient outerColors, Gradient innerColors)
        {
            int width = Math.Min(outerIndicies.GetLength(0), innerIndicies.GetLength(0));
            int height = Math.Min(outerIndicies.GetLength(1), innerIndicies.GetLength(1));

            InitializeImage(width, height);
            Parallel.For(0, height, y =>
            {
                Parallel.For(0, width, x =>
                {
                    if (double.IsNaN(outerIndicies[x, y]) && !double.IsNaN(innerIndicies[x, y]))
                        WritePixel(x, y, innerColors[innerIndicies[x, y]]);
                    else if (!double.IsNaN(outerIndicies[x, y]))
                        WritePixel(x, y, outerColors[outerIndicies[x, y]]);
                    else
                        WritePixel(x, y, new RgbaValue(0, 0, 0));
                });
            });
        }

        public abstract void InitializeImage(int width, int height);
        public abstract void WritePixel(int x, int y, RgbaValue color);
    }
}
