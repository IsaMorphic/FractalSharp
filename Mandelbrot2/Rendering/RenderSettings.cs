using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Rendering
{
    class RenderSettings
    {
        public int NumFrames = 0;
        public long MaxIterations = 100;
        public int ThreadCount = Environment.ProcessorCount;

        public decimal offsetX = -0.743643887037158704752191506114774M;
        public decimal offsetY = 0.131825904205311970493132056385139M;

        public int Width = 640;
        public int Height = 480;
    }
}
