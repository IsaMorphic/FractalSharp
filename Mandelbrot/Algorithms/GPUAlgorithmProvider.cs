using ManagedCuda;
using ManagedCuda.VectorTypes;
using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Algorithms
{
    abstract class GPUAlgorithmProvider<T> : IAlgorithmProvider<T>
    {
        protected CudaKernel gpuKernel;

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

        public abstract void Init(IGenericMath<T> TMath, decimal offsetX, decimal offsetY, int maxIterations);
        public abstract PixelData<T> Run(T x0, T y0);
    }
}
