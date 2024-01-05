/*
 *  Copyright 2018-2024 Chosen Few Software
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
    public abstract class UpscalingImageBuilder : ImageBuilder
    {
        public void CreateImage(double[,] outerIndicies, double[,] innerIndicies, Gradient outerColors, Gradient innerColors, int newWidth, int newHeight)
        {
            int oldWidth = Math.Min(outerIndicies.GetLength(1), innerIndicies.GetLength(1));
            int oldHeight = Math.Min(outerIndicies.GetLength(0), innerIndicies.GetLength(0));

            int scaleX = newWidth / oldWidth;
            int scaleY = newHeight / oldHeight;

            InitializeImage(newWidth, newHeight);
            Parallel.For(0, newHeight, y =>
            {
                int scaledY = Math.Min(y / scaleY, oldHeight - 1);
                Parallel.For(0, newWidth, x =>
                {
                    int scaledX = Math.Min(x / scaleX, oldWidth - 1);
                    if (double.IsNaN(outerIndicies[scaledY, scaledX]))
                        WritePixel(x, y, innerColors[innerIndicies[scaledY, scaledX]]);
                    else
                        WritePixel(x, y, outerColors[outerIndicies[scaledY, scaledX]]);
                });
            });
        }
    }
}
