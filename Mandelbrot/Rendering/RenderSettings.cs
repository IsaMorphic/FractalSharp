using Mandelbrot.Algorithms;
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
        private Type _algorithmType = typeof(TraditionalAlgorithmProvider<>);

        private int    _maxIterations = 100;
        private int    _threadCount   = Environment.ProcessorCount;
        private double _magnification = 1;

        private decimal _offsetX = -0.743643887037158704752191506114774M;
        private decimal _offsetY =  0.131825904205311970493132056385139M;

        private int _width  = 960;
        private int _height = 540;

        private bool _gradual = false;

        private int[] _maxChunkSizes = new int[12] 
        {
            1, 1, 1, 1,
            1, 1, 1, 1,
            1, 1, 1, 1,
        };

        public virtual Type AlgorithmType { get => _algorithmType; set => _algorithmType = value; }
        public virtual int MaxIterations { get => _maxIterations; set => _maxIterations = value; }
        public virtual int ThreadCount { get => _threadCount; set => _threadCount = value; }
        public virtual double Magnification { get => _magnification; set => _magnification = value; }
        public virtual decimal offsetX { get => _offsetX; set => _offsetX = value; }
        public virtual decimal offsetY { get => _offsetY; set => _offsetY = value; }
        public virtual int Width { get => _width; set => _width = value; }
        public virtual int Height { get => _height; set => _height = value; }
        public virtual bool Gradual { get => _gradual; set => _gradual = value; }
        public virtual int[] MaxChunkSizes { get => _maxChunkSizes; set => _maxChunkSizes = value; }
    }
}
