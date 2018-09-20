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
        int[] GPUFrame(int[] palette, int width, int height, double xMax, double yMax, double offsetX, double offsetY, int maxIter);
        void GPUCleanup();
    }
}
