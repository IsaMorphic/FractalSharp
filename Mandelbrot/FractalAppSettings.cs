using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mandelbrot.Rendering;

namespace Mandelbrot
{
    class FractalAppSettings : RenderSettings
    {
        public string palettePath;
        public string videoPath;

        public bool extraPrecision;
        public int version = 0;
    }
}
