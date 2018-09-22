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
    interface IAlgorithmProvider<T>
    {
        void Init(IGenericMath<T> TMath, decimal offsetX, decimal offsetY, int maxIterations);

        PixelData<T> Run(T x0, T y0);

        void GPUInit(CudaContext ctx, byte[] ptxImage, dim3 gridDim, dim3 blockDim);
        void GPUPreFrame();
        void GPUPostFrame();
        void GPUCell(
            CudaDeviceVariable<int> dev_image,
            CudaDeviceVariable<int> dev_palette,
            int cell_x, int cell_y,
            int cellWidth, int cellHeight,
            int totalCells_x, int totalCells_y,
            double xMax, double yMax, 
            int chunkSize, int maxChunkSize);

    }
}
