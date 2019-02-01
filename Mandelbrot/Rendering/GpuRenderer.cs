using ManagedCuda;
using ManagedCuda.VectorTypes;
using Mandelbrot.Algorithms;
using Mandelbrot.Mathematics;
using Mandelbrot.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Rendering
{
    class GpuRenderer : MandelbrotRenderer
    {
        private int[] int_palette;
        private GPUAlgorithmProvider<double> GPUAlgorithmProvider;
        private CudaContext ctx;


        public bool GPUAvailable()
        {
            bool cudaAvailable =
                CudaContext.GetDeviceCount() > 0 &&
                Environment.Is64BitOperatingSystem;

            return cudaAvailable;
        }

        public bool InitGPU()
        {
            if (isInitialized)
            {
                if (!GPUAvailable())
                    return false;

                ctx = new CudaContext(CudaContext.GetMaxGflopsDeviceId());

                int_palette = new int[palette.Length];
                for (var i = 0; i < palette.Length; i++)
                {
                    int_palette[i] = palette[i].toColor().ToArgb();
                }

                Type algorithmType = AlgorithmType.MakeGenericType(typeof(double));

                try
                {
                    GPUAlgorithmProvider =
                        (GPUAlgorithmProvider<double>)Activator
                        .CreateInstance(algorithmType);
                }
                catch (InvalidCastException)
                {
                    throw new ApplicationException("The selected algorithm does not support GPU Acceleration");
                }

                GPUAlgorithmProvider.GPUInit(ctx, Resources.Kernel, new dim3(Width / 16, Height / 9), new dim3(4, 3));

                return true;
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized");
            }
        }

        public void CleanupGPU()
        {
            try
            {
                ctx.Dispose();
            }
            catch (NullReferenceException) { }
        }

        private void RenderGPUCell(CudaDeviceVariable<int> dev_image, CudaDeviceVariable<int> dev_palette)
        {
            int index = CellX + CellY * 4;
            int chunkSize = ChunkSizes[index];
            int maxChunkSize = MaxChunkSizes[index];

            double xMax = (double)(aspectM / Magnification);
            double yMax = (double)(2 / Magnification);

            GPUAlgorithmProvider.GPUCell(
                dev_image,
                dev_palette,
                CellX, CellY,
                CellWidth, CellHeight,
                TotalCellsX, TotalCellsY,
                xMax, yMax,
                chunkSize, maxChunkSize);

            if (chunkSize > 1)
                ChunkSizes[index] = chunkSize / 2;
        }

        private async Task<int[]> RenderGPUCells()
        {
            ctx.SetCurrent();

            GPUAlgorithmProvider.GPUPreFrame();

            CudaDeviceVariable<int> dev_palette = int_palette;
            CudaDeviceVariable<int> dev_image = CurrentFrame.Bits;

            if (Gradual)
            {
                IncrementCellCoords();
                RenderGPUCell(dev_image, dev_palette);
            }
            else
            {
                for (CellX = 0; CellX < TotalCellsX; CellX++)
                {
                    for (CellY = 0; CellY < TotalCellsY; CellY++)
                    {
                        RenderGPUCell(dev_image, dev_palette);
                    }
                }
            }

            int[] raw_image = dev_image;

            GPUAlgorithmProvider.GPUPostFrame();

            dev_palette.Dispose();
            dev_image.Dispose();

            return raw_image;
        }

        public void RenderFrameGPU()
        {
            FrameStart();

            IGenericMath<double> TMath = MathResolver.CreateMathObject<double>();
            GPUAlgorithmProvider.Init(TMath, new RenderSettings { Magnification = Magnification, offsetX = offsetXM, offsetY = offsetYM, MaxIterations = MaxIterations });

            var renderTask = Task.Run(RenderGPUCells, Job.Token);

            renderTask.Wait();

            int[] raw_image = renderTask.Result;

            CurrentFrame.SetBits(raw_image);

            Bitmap NewFrame = (Bitmap)CurrentFrame.Bitmap.Clone();

            FrameEnd(NewFrame);
        }
    }
}
