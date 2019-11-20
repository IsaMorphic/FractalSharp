﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Processing;

namespace MandelbrotSharp.Imaging
{
    public class ColorProcessorConfig : ProcessorConfig
    {
        public PointData[,] InputData { get; set; }

        public override ProcessorConfig Copy()
        {
            return new ColorProcessorConfig
            {
                ThreadCount = ThreadCount,
                Params = Params.Copy(),
                InputData = InputData.Clone() as PointData[,]
            };
        }
    }

    public class ColorProcessor<TAlgorithm> : BaseProcessor<PointData, double, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<PointData, double>, new()
    {
        protected new ColorProcessorConfig Settings => base.Settings as ColorProcessorConfig;

        public ColorProcessor(int width, int height) : base(width, height)
        {
        }

        protected override double[,] Process(ParallelOptions options)
        {
            double[,] indicies = new double[Height, Width];

            Parallel.For(0, Height, y => 
            {
                Parallel.For(0, Width, x =>
                {
                    PointData pointData = Settings.InputData[y, x];
                    indicies[y, x] = pointData.Escaped ? AlgorithmProvider.Run(pointData) : double.NaN;
                });
            });

            return indicies;
        }
    }
}