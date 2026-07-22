/*
 *  Copyright 2018-2026 Chosen Few Software
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

using FractalSharp.Algorithms;
using FractalSharp.Algorithms.Fractals;
using FractalSharp.Numerics.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class FractalProcessor<TAlgorithm, TNumber>
        : BaseProcessor<Complex<TNumber>, PointData<double>>
        where TAlgorithm : IFractalProvider<EscapeTimeParams<TNumber>, TNumber>
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>
    {
        protected PointMapper<TNumber> pointMapper;

        public FractalProcessor(int width, int height) : base(width, height)
        {
            pointMapper = new PointMapper<TNumber>();
            pointMapper.InputSpace = new Rectangle<TNumber>(TNumber.Zero, TNumber.CreateSaturating((double)Width), TNumber.Zero, TNumber.CreateSaturating((double)Height));
        }

        public override async Task SetupAsync(ProcessorConfig settings, CancellationToken cancellationToken)
        {
            await base.SetupAsync(settings, cancellationToken);

            EscapeTimeParams<TNumber> @params = (Settings as ProcessorConfig<EscapeTimeParams<TNumber>>)?.Params ?? default;
            TNumber aspectRatio = TNumber.CreateSaturating((double)Width) / TNumber.CreateSaturating((double)Height);

            pointMapper.OutputSpace = TAlgorithm.GetOutputBounds(@params, aspectRatio);
        }

        protected override PointData<double>[,] Process(ParallelOptions options)
        {
            EscapeTimeParams<TNumber> @params = (Settings as ProcessorConfig<EscapeTimeParams<TNumber>>)?.Params ?? default;
            PointData<double>[,] data = new PointData<double>[Width, Height];

            Parallel.For(0, Height, options, y =>
            {
                var py = pointMapper.MapPointY(TNumber.CreateSaturating((double)y));
                Parallel.For(0, Width, options, x =>
                {
                    var px = pointMapper.MapPointX(TNumber.CreateSaturating((double)x));
                    data[x, y] = TAlgorithm.Run(@params, new Complex<TNumber>(px, py));
                });
            });

            return data;
        }
    }
}
