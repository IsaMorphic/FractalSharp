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
using MandelbrotSharp.Numerics;
using System;
using System.Threading;

namespace MandelbrotSharp.Algorithms.Coloring
{
    public class SmoothColoringAlgorithm : AlgorithmProvider<PointData, double, EmptyColoringParams>
    {
        protected override bool Initialize(CancellationToken cancellationToken) => true;

        public override double Run(PointData data)
        {
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(Complex<double>.AbsSqu(data.ZValue).Value) / 2;
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
