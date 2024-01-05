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

using FractalSharp.Numerics.Generic;
using System;

namespace FractalSharp.Algorithms.Coloring
{
    public class SmoothColoringAlgorithm : IAlgorithmProvider<PointData<double>, double, EmptyColoringParams>
    {
        public static double Run(EmptyColoringParams @params, PointData<double> data)
        {
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(Complex<double>.AbsSqu(data.ZValue)) / 2;
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            // Rearranging the potential function.
            // Dividing log_zn by log(2) instead of log(N = 1<<8)
            // because we want the entire palette to range from the
            // center to radius 2, NOT our bailout radius.

            // Return the result.
            return data.IterCount + 1 - nu;
        }
    }
}
