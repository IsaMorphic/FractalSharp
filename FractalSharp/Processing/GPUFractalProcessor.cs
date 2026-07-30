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
using FractalSharp.Numerics.Generic;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class GPUFractalProcessor<TAlgorithm, TParams, TNumber> : FractalProcessor<TAlgorithm, TParams, TNumber>, IDisposable
        where TAlgorithm : IFractalProvider<TParams, TNumber>
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>
        where TParams : unmanaged, IFractalProviderParams<TNumber>
    {
        private static void FractalKernel(Index2D idx, Index2D offset, ArrayView2D<Complex<TNumber>, Stride2D.DenseY> inputBuff, ArrayView2D<PointData<double>, Stride2D.DenseY> outputBuff, VariableView<TParams> @params)
        {
            outputBuff[offset + idx] = TAlgorithm.Run(@params.Value, inputBuff[offset + idx]);
        }

        private readonly Context context;

        private readonly Accelerator accelerator;

        private readonly Complex<TNumber>[,] cpuInputBuffer;

        private readonly PointData<double>[,] cpuOutputBuffer;

        private readonly MemoryBuffer2D<Complex<TNumber>, Stride2D.DenseY> gpuInputBuffer;

        private readonly MemoryBuffer2D<PointData<double>, Stride2D.DenseY> gpuOutputBuffer;

        private readonly MemoryBuffer1D<TParams, Stride1D.Dense> gpuVariableBuffer;

        private readonly Action<Index2D, Index2D, ArrayView2D<Complex<TNumber>, Stride2D.DenseY>, ArrayView2D<PointData<double>, Stride2D.DenseY>, VariableView<TParams>> loadedKernel;

        private bool disposedValue;

        public GPUFractalProcessor(int width, int height) : base(width, height)
        {
            context = Context.Create().Default()
                .EnableAlgorithms()
                .ToContext();

            Device device = context.GetPreferredDevice(false);
            accelerator = device.CreateAccelerator(context);

            cpuInputBuffer = new Complex<TNumber>[Width, Height];
            cpuOutputBuffer = new PointData<double>[Width, Height];

            gpuInputBuffer = accelerator.Allocate2DDenseY<Complex<TNumber>>(new LongIndex2D(Width, Height));
            gpuOutputBuffer = accelerator.Allocate2DDenseY<PointData<double>>(new LongIndex2D(Width, Height));
            gpuVariableBuffer = accelerator.Allocate1D<TParams>(1L);

            loadedKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, Index2D, ArrayView2D<Complex<TNumber>, Stride2D.DenseY>, ArrayView2D<PointData<double>, Stride2D.DenseY>, VariableView<TParams>>(FractalKernel);
        }

        protected override PointData<double>[,] Process(ParallelOptions options)
        {
            if (accelerator is null || loadedKernel is null || Settings is null)
            {
                throw new InvalidOperationException();
            }

            Parallel.For(0, Height, options, y =>
            {
                var py = pointMapper.MapPointY(TNumber.CreateSaturating((double)y));
                Parallel.For(0, Width, options, x =>
                {
                    var px = pointMapper.MapPointX(TNumber.CreateSaturating((double)x));
                    cpuInputBuffer[x, y] = new Complex<TNumber>(px, py);
                });
            });
            gpuInputBuffer.CopyFromCPU(cpuInputBuffer);

            gpuVariableBuffer.CopyFromCPU([(TParams)Settings.Params!]);
            RunKernel((Width, Height), options.CancellationToken);

            gpuOutputBuffer.CopyToCPU(cpuOutputBuffer);
            return cpuOutputBuffer;
        }

        private void RunKernel(Index2D size, CancellationToken ct, Index2D offset = default, int depth = 0)
        {
            if (size.X > 256 || size.Y > 256)
            {
                int adj = (depth >> 1) & 1;
                if ((depth & 1) == 0)
                {
                    int sizeA = size.X / 2 + adj;
                    int sizeB = size.X - sizeA;
                    RunKernel((sizeA, size.Y), ct, offset, depth + 1);
                    RunKernel((sizeB, size.Y), ct, offset + (sizeA, 0), depth + 1);
                }
                else
                {
                    int sizeA = size.Y / 2 + adj;
                    int sizeB = size.Y - sizeA;
                    RunKernel((size.X, sizeA), ct, offset, depth + 1);
                    RunKernel((size.X, sizeB), ct, offset + (0, sizeA), depth + 1);
                }
            }
            else
            {
                ct.ThrowIfCancellationRequested();

                VariableView<TParams> @params = gpuVariableBuffer.View.VariableView(0);
                loadedKernel(size, offset, gpuInputBuffer, gpuOutputBuffer, @params);

                accelerator.Synchronize();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    gpuInputBuffer.Dispose();
                    gpuOutputBuffer.Dispose();
                    gpuVariableBuffer.Dispose();

                    accelerator.Dispose();
                    context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
