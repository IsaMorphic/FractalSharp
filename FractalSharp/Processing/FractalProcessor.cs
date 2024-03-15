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

using FractalSharp.Algorithms;
using FractalSharp.Numerics.Generic;
using FractalSharp.Numerics.Helpers;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class FractalProcessor<TAlgorithm, TParams, TNumber>
        : BaseProcessor<Complex<TNumber>, PointData<double>, TAlgorithm, TParams>
        where TAlgorithm : IFractalProvider<TParams, TNumber>
        where TParams : struct
        where TNumber : struct, INumber<TNumber>
    {
        protected PointMapper<TNumber> pointMapper;

        public FractalProcessor(int width, int height) : base(width, height)
        {
            pointMapper = new PointMapper<TNumber>();
            pointMapper.InputSpace = new Rectangle<TNumber>(TNumber.Zero, TNumber.CreateChecked((double)Width), TNumber.Zero, TNumber.CreateChecked((double)Height));
        }

        public override async Task SetupAsync(ProcessorConfig<TParams> settings, CancellationToken cancellationToken)
        {
            await base.SetupAsync(settings, cancellationToken);

            TParams @params = Settings?.Params ?? default;
            TNumber aspectRatio = TNumber.CreateChecked((double)Width) / TNumber.CreateChecked((double)Height);

            pointMapper.OutputSpace = TAlgorithm.GetOutputBounds(@params, aspectRatio);
        }

        protected override PointData<double>[,] Process(ParallelOptions options)
        {
            TParams @params = Settings?.Params ?? default;
            PointData<double>[,] data = new PointData<double>[Width, Height];

            Parallel.For(0, Height, options, y =>
            {
                var py = pointMapper.MapPointY(TNumber.CreateChecked((double)y));
                Parallel.For(0, Width, options, x =>
                {
                    var px = pointMapper.MapPointX(TNumber.CreateChecked((double)x));
                    data[x, y] = TAlgorithm.Run(@params, new Complex<TNumber>(px, py));
                });
            });

            return data;
        }
    }
}
