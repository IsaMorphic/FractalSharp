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
using FractalSharp.Algorithms.Fractals;
using FractalSharp.Numerics.Generic;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class GPUFractalProcessor<TAlgorithm, TNumber> : FractalProcessor<TAlgorithm, EscapeTimeParams<TNumber>, TNumber>, IDisposable
        where TAlgorithm : 
            IFractalProvider<EscapeTimeParams<TNumber>, TNumber>, 
            IAlgorithmProvider<Complex<TNumber>, PointData<double>, SpecializedValue<int>>
        where TNumber : unmanaged, INumber<TNumber>
    {
        private static void FractalKernel(Index2D idx, ArrayView2D<Complex<TNumber>, Stride2D.DenseY> inputBuff, ArrayView2D<PointData<double>, Stride2D.DenseY> outputBuff, SpecializedValue<int> maxIterations)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    outputBuff[new(idx.X * 4 + x, idx.Y * 4 + y)] = TAlgorithm.Run(maxIterations, inputBuff[new(idx.X * 4 + x, idx.Y * 4 + y)]);
                }
            }
        }

        private Context context;
        private Accelerator accelerator;
        private Action<Index2D, ArrayView2D<Complex<TNumber>, Stride2D.DenseY>, ArrayView2D<PointData<double>, Stride2D.DenseY>, SpecializedValue<int>> loadedKernel;

        private bool disposedValue;

        public GPUFractalProcessor(int width, int height) : base(width, height)
        {
            context = Context.Create()
                .Default()
                .Cuda(dev => true)
                .ToContext();
            accelerator = context.CreateCudaAccelerator(0);
            loadedKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<Complex<TNumber>, Stride2D.DenseY>, ArrayView2D<PointData<double>, Stride2D.DenseY>, SpecializedValue<int>>(FractalKernel);
        }

        protected override PointData<double>[,] Process(ParallelOptions options)
        {
            if (accelerator is null || loadedKernel is null || Settings is null)
            {
                throw new InvalidOperationException();
            }

            Complex<TNumber>[,] cpuInputBuffer = new Complex<TNumber>[Width, Height];
            Parallel.For(0, Height, options, y =>
            {
                var py = pointMapper.MapPointY(TNumber.CreateChecked((double)y));
                Parallel.For(0, Width, options, x =>
                {
                    var px = pointMapper.MapPointX(TNumber.CreateChecked((double)x));
                    cpuInputBuffer[x, y] = new Complex<TNumber>(px, py);
                });
            });

            PointData<double>[,] cpuOutputBuffer = new PointData<double>[Width, Height];

            using (var gpuInputBuffer = accelerator.Allocate2DDenseY(cpuInputBuffer))
            using (var gpuOutputBuffer = accelerator.Allocate2DDenseY(cpuOutputBuffer))
            {

                loadedKernel(new (Width / 4, Height / 4), gpuInputBuffer, gpuOutputBuffer, SpecializedValue.New(Settings.Params.MaxIterations));

                accelerator.Synchronize();
                gpuOutputBuffer.CopyToCPU(cpuOutputBuffer);
            }

            return cpuOutputBuffer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    accelerator?.Dispose();
                    context?.Dispose();
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
