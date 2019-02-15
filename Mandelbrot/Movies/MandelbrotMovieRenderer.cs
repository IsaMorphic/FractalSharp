using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mandelbrot.Rendering;

namespace Mandelbrot.Movies
{
    class MandelbrotMovieRenderer : MandelbrotRenderer
    {

        public int NumFrames { get; private set; }

        public void Setup(ZoomMovieSettings settings)
        {
            base.Setup(settings);
            NumFrames = settings.NumFrames;
            Magnification = BigDecimal.Pow(2, NumFrames / 64.0);
        }

        public void SetFrame(int frameNum)
        {
            // Set variables and get new zoom value.  
            NumFrames = frameNum;

            Magnification = Math.Pow(2, NumFrames / 64.0);
        }
    }
}
