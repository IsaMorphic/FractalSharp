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
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Processing
{
    public class FractalProcessor<TNumber, TAlgorithm>
        : BaseProcessor<Complex<TNumber>, PointData, TAlgorithm>
        where TAlgorithm : IFractalProvider<TNumber>, new()
        where TNumber : struct
    {
        protected PointMapper<int, TNumber> PointMapper { get; private set; }

        public FractalProcessor(int width, int height) : base(width, height)
        {
            PointMapper = new PointMapper<int, TNumber>();
            PointMapper.InputSpace = new Rectangle<int>(0, Width, 0, Height);
        }

        public override async Task SetupAsync(ProcessorConfig settings, CancellationToken cancellationToken)
        {
            await base.SetupAsync(settings, cancellationToken);
            Number<TNumber> aspectRatio = Number<TNumber>.From(Width) / Number<TNumber>.From(Height);
            PointMapper.OutputSpace = AlgorithmProvider.GetOutputBounds(aspectRatio);
        }

        protected override PointData[,] Process(ParallelOptions options)
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
