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
using System.Threading.Tasks;
using FractalSharp.Algorithms;
using FractalSharp.Processing;

namespace FractalSharp.Imaging
{
    public class ColorProcessorConfig<TParams> : ProcessorConfig<TParams>
        where TParams : struct
    {
        public PointClass PointClass { get; init; }
        public PointData[,] InputData { get; init; }

        public override ProcessorConfig<TParams> Copy()
        {
            return new ColorProcessorConfig<TParams>
            {
                ThreadCount = ThreadCount,
                Params = Params,
                PointClass = PointClass,
                InputData = (PointData[,])InputData.Clone()
            };
        }
    }

    public class ColorProcessor<TAlgorithm, TParams> : BaseProcessor<PointData, double, TAlgorithm, TParams>
        where TAlgorithm : IAlgorithmProvider<PointData, double, TParams>
        where TParams : struct
    {
        protected new ColorProcessorConfig<TParams>? Settings => base.Settings as ColorProcessorConfig<TParams>;

        public ColorProcessor(int width, int height) : base(width, height)
        {
        }

        protected override double[,] Process(ParallelOptions options)
        {
            if (Settings is null) throw new InvalidOperationException();

            double[,] indicies = new double[Height, Width];

            Parallel.For(0, Height, y =>
            {
                Parallel.For(0, Width, x =>
                {
                    PointData pointData = Settings.InputData[y, x];
                    if (pointData.PointClass == Settings.PointClass)
                        indicies[y, x] = TAlgorithm.Run(Settings.Params, pointData);
                    else
                        indicies[y, x] = double.NaN;
                });
            });

            return indicies;
        }
    }
}
