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
using MandelbrotSharp.Numerics;
using System.Threading.Tasks;

namespace MandelbrotSharp.Processing
{
    public class DefaultProcessor<TNumber, TAlgorithm> : FractalProcessor<TNumber, TAlgorithm>
        where TAlgorithm : IFractalProvider<TNumber>, new()
        where TNumber : struct
    {
        public DefaultProcessor(int width, int height) : base(width, height)
        {
        }

        protected override PointData[,] RenderFrame(ParallelOptions options)
        {
            PointData[,] data = new PointData[Height, Width];

            Parallel.For(0, Height, options, y =>
            {
                var py = PointMapper.MapPointY(y);
                Parallel.For(0, Width, options, x =>
                {
                    var px = PointMapper.MapPointX(x);
                    data[y, x] = AlgorithmProvider.Run(new Complex<TNumber>(px, py));
                });
            });

            return data;
        }
    }
}
