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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public class DefaultRenderer<TNumber, TAlgorithm> : FractalRenderer<TNumber, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<TNumber>, new()
        where TNumber : struct
    {
        public DefaultRenderer(int width, int height) : base(width, height)
        {
        }

        protected override void RenderFrame(ParallelOptions options)
        {
            Parallel.For(0, Height, options, py =>
            {
                var y0 = PointMapper.MapPointY(py);

                Parallel.For(0, Width, options, px =>
                {
                    var x0 = PointMapper.MapPointX(px);

                    PointData pointData = AlgorithmProvider.Run(new Complex<TNumber>(x0, y0));

                    if (pointData.Escaped)
                    {
                        double colorIndex = PointColorer.GetIndexFromPointData(pointData);
                        CurrentFrame.SetPixel(px, py, Settings.OuterColors[colorIndex]);
                    }
                    else
                    {
                        CurrentFrame.SetPixel(px, py, Settings.InnerColor);
                    }
                });
            });
        }
    }
}
