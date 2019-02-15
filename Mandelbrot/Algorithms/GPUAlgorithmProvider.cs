using ManagedCuda;
using ManagedCuda.VectorTypes;
using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using Mandelbrot.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Algorithms
{
    /*abstract class GPUAlgorithmProvider<T> : AlgorithmProvider<T>
    {
        protected CudaKernel gpuKernel;

        public GPUAlgorithmProvider(object TMath, RenderSettings settings) : base(TMath, settings)
        {
        }

        public abstract void GPUInit(CudaContext ctx, byte[] ptxImage, dim3 gridDim, dim3 blockDim);
        public abstract void GPUPreFrame();
        public abstract void GPUPostFrame();
        public abstract void GPUCell(
            CudaDeviceVariable<int> dev_image,
            CudaDeviceVariable<int> dev_palette,
            int cell_x, int cell_y,
            int cellWidth, int cellHeight,
            int totalCells_x, int totalCells_y,
            double xMax, double yMax,
            int chunkSize, int maxChunkSize);

        public override abstract void Init();
        public override abstract PixelData Run(T x0, T y0);
    }*/
}
