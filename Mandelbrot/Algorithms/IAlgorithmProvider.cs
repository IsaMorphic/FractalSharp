using ManagedCuda;
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
        void Init(IGenericMath<T> TMath, T offsetX, T offsetY, int maxIterations);

        PixelData<T> Run(T x0, T y0);

        void GPUInit(CudaContext ctx);
        void GPUPreFrame();
        void GPUPostFrame();
        void GPUCell(
            CudaDeviceVariable<int> dev_image, 
            CudaDeviceVariable<int> dev_palette, 
            int cell_x, int cell_y, 
            int cellWidth, int cellHeight, 
            double xMax, double yMax);

    }
}
