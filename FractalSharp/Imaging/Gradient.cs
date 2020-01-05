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

namespace FractalSharp.Imaging
{
    public struct Gradient
    {
        public RgbaValue[] KeyPoints { get; }
        public int Length { get; }

        public RgbaValue this[double index]
        {
            get
            {
                double scaled = index / Length % 1 * (KeyPoints.Length - 1);
                int firstIndex = (int)scaled;
                int secondIndex = firstIndex + 1;
                double alpha = scaled % 1;
                return RgbaValue.LerpColors(KeyPoints[firstIndex], KeyPoints[secondIndex], alpha);
            }
        }

        public Gradient(RgbaValue[] keyPoints, int length)
        {
            Length = length;

            KeyPoints = new RgbaValue[keyPoints.Length];
            Array.Copy(keyPoints, KeyPoints, KeyPoints.Length);
        }
    }
}
