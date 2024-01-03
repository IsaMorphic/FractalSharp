using FractalSharp.Algorithms;
using FractalSharp.Numerics.Generic;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.Processing
{
    public class GPUFractalProcessor<TAlgorithm, TParams, TNumber> : FractalProcessor<TAlgorithm, TParams, TNumber>, IDisposable
        where TAlgorithm : IFractalProvider<TParams, TNumber>
        where TParams : struct
        where TNumber : struct, INumber
    {
        private static void FractalKernel(Index2D idx, ArrayView2D<PointData, Stride2D.DenseY> buff, PointMapper<TNumber> pointMapper, TParams @params)
        {
            var py = pointMapper.MapPointY(idx.Y);
            var px = pointMapper.MapPointX(idx.X);
            buff[idx] = TAlgorithm.Run(@params, new Complex<TNumber>(px, py));
        }

        private Context? context;
        private Accelerator? accelerator;
        private Action<Index2D, ArrayView2D<PointData, Stride2D.DenseY>, PointMapper<TNumber>, TParams>? loadedKernel;

        private bool disposedValue;

        public GPUFractalProcessor(int width, int height) : base(width, height)
        {
        }

        public override async Task SetupAsync(ProcessorConfig<TParams> settings, CancellationToken cancellationToken)
        {
            await base.SetupAsync(settings, cancellationToken);

            context = Context.CreateDefault();
            accelerator = context.GetPreferredDevice(preferCPU: false)
                .CreateAccelerator(context);

            loadedKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<PointData, Stride2D.DenseY>, PointMapper<TNumber>, TParams>(FractalKernel);
        }

        protected override PointData[,] Process(ParallelOptions options)
        {
            if (accelerator is null || loadedKernel is null || Settings is null)
            {
                throw new InvalidOperationException();
            }

            var gpuBuffer = accelerator.Allocate2DDenseY<PointData>(new LongIndex2D(Width, Height));

            loadedKernel(gpuBuffer.IntExtent, gpuBuffer, pointMapper, Settings.Params);
            accelerator.Synchronize();

            var cpuBuffer = new PointData[Height, Width];
            gpuBuffer.CopyToCPU(cpuBuffer);

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
