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
            Magnification = Math.Pow(NumFrames, NumFrames / 100.0);
        }

        public void SetFrame(int frameNum)
        {
            // Set variables and get new zoom value.  
            NumFrames = frameNum;

            Magnification = Math.Pow(NumFrames, NumFrames / 100.0);

            MaxIterations += NumFrames / Math.Max(5 - NumFrames / 100, 1);

        }
    }
}
