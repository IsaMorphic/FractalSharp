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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public interface IProcessor<TOutput, TParams>
        where TParams : struct
    {
        int Width { get; }
        int Height { get; }

        Task SetupAsync(ProcessorConfig<TParams> settings, CancellationToken cancellationToken);
        Task<TOutput[,]> ProcessAsync(CancellationToken cancellationToken);
    }

    public abstract class BaseProcessor<TInput, TOutput, TAlgorithm, TParams> : IProcessor<TOutput, TParams>
        where TAlgorithm : IAlgorithmProvider<TInput, TOutput, TParams>
        where TParams : struct
    {
        public BaseProcessor(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        protected ProcessorConfig<TParams>? Settings { get; private set; }

        public virtual Task SetupAsync(ProcessorConfig<TParams> settings, CancellationToken cancellationToken)
        {
            Settings = settings.Copy();
            return Task.CompletedTask;
        }

        public Task<TOutput[,]> ProcessAsync(CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Settings?.ThreadCount ?? throw new InvalidOperationException(),
                CancellationToken = cancellationToken
            };
            return Task.Run(() => Process(options));
        }

        protected abstract TOutput[,] Process(ParallelOptions options);
    }
}
