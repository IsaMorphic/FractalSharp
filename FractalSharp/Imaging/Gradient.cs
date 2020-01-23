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
using System.Collections.Generic;

namespace FractalSharp.Imaging
{
    public class Gradient
    {
        public int Length { get; set; }
        public List<GradientKey> Keys { get; set; }

        public RgbaValue this[double index]
        {
            get
            {
                double scaled = index / Length % 1 * (Keys.Count - 1);
                int firstIndex = (int)scaled;
                int secondIndex = firstIndex + 1;
                double alpha = scaled % 1;
                return RgbaValue.LerpColors(Keys[firstIndex].Color, Keys[secondIndex].Color, alpha);
            }
        }

        public Gradient(int length, List<GradientKey> keys)
        {
            Length = length;
            Keys = keys;
        }
    }
}
