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
using ILGPU;
using ILGPU.IR.Types;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class GPUFractalProcessor<TAlgorithm, TParams, TNumber, TConverter> : FractalProcessor<TAlgorithm, TParams, TNumber>, IDisposable
        where TAlgorithm : IFractalProvider<TParams, TNumber>
        where TParams : struct
        where TNumber : unmanaged, INumber<TNumber>
        where TConverter : struct, INumberConverter<TNumber>
    {
        private static void FractalKernel(Index2D idx, ArrayView2D<PointData<double>, Stride2D.DenseY> buff, PointMapper<TNumber> pointMapper, TParams @params)
        {
            TConverter floatConverter = default;
            var py = pointMapper.MapPointY(floatConverter.FromInt32(idx.Y));
            var px = pointMapper.MapPointX(floatConverter.FromInt32(idx.X));

            buff[idx] = TAlgorithm.Run(@params, new Complex<TNumber>(px, py));
        }

        private Context context;
        private Accelerator accelerator;
        private Action<Index2D, ArrayView2D<PointData<double>, Stride2D.DenseY>, PointMapper<TNumber>, TParams> loadedKernel;

        private bool disposedValue;

        public GPUFractalProcessor(int width, int height) : base(width, height)
        {
            context = Context.Create()
                .Default()
                .Optimize(OptimizationLevel.O0)
                .ToContext();
            accelerator = context.GetPreferredDevice(preferCPU: false)
                .CreateAccelerator(context);

            loadedKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<PointData<double>, Stride2D.DenseY>, PointMapper<TNumber>, TParams>(FractalKernel);
        }

        protected override PointData<double>[,] Process(ParallelOptions options)
        {
            if (accelerator is null || loadedKernel is null || Settings is null)
            {
                throw new InvalidOperationException();
            }

            PointData<double>[,] cpuBuffer;
            using (var gpuBuffer = accelerator.Allocate2DDenseY<PointData<double>>(new LongIndex2D(Width, Height)))
            {

                loadedKernel(gpuBuffer.IntExtent, gpuBuffer, pointMapper, Settings.Params);
                cpuBuffer = new PointData<double>[Width, Height];

                accelerator.Synchronize();
                gpuBuffer.CopyToCPU(cpuBuffer);
            }

            return cpuBuffer;
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
