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
    interface IAlgorithmProvider<T>
    {
        void Init(IGenericMath<T> TMath, RenderSettings settings);

        PixelData Run(T x0, T y0);
    }
}
