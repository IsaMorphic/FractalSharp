using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Movies
{
    class MandelbrotMovieRenderer : MandelbrotRenderer
    {

        public int NumFrames { get; private set; }
        public int MaxIterations { get => base.MaxIterations; }
        public BigDecimal Magnification { get => base.Magnification; }

        public void Setup(ZoomMovieSettings settings)
        {
            base.Setup(settings);
            NumFrames = settings.NumFrames;
            base.Magnification = BigDecimal.Pow(2, NumFrames / 30.0);
        }

        public void SetFrame(int frameNum)
        {
            // Set variables and get new zoom value.  
            NumFrames = frameNum;

            base.Magnification = Math.Pow(2, NumFrames / 30.0);
        }
    }
}
