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
    abstract class AlgorithmProvider<T>
    {
        protected readonly GenericMath<T> TMath;
        protected readonly RenderSettings Settings;

        public AlgorithmProvider(object TMath, RenderSettings settings)
        {
            this.TMath = TMath as GenericMath<T>;
            this.Settings = settings;
            Init();
        }

        public abstract void Init();

        public abstract PixelData Run(BigDecimal x0, BigDecimal y0);
    }
}
