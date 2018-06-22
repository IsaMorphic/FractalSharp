using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mandelbrot.Rendering;

namespace Mandelbrot.Movies
{
    class ZoomMovieSettings : RenderSettings
    {
        public int NumFrames = 0;

        public string PalettePath;
        public string VideoPath;

        public bool ExtraPrecision;
        public int Version = 0;
    }
}
