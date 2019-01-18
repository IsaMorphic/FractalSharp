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
        protected Type _AlgorithmType = typeof(TraditionalAlgorithmProvider<>);

        protected int    _MaxIterations = 100;
        protected int    _ThreadCount   = Environment.ProcessorCount;
        protected double _Magnification = 1;

        protected decimal _offsetX = -0.743643887037158704752191506114774M;
        protected decimal _offsetY =  0.131825904205311970493132056385139M;

        protected int _Width  = 960;
        protected int _Height = 540;

        protected bool _Gradual = false;

        protected int[] _MaxChunkSizes = new int[12] 
        {
            1, 1, 1, 1,
            1, 1, 1, 1,
            1, 1, 1, 1,
        };

        public virtual Type AlgorithmType { get => _AlgorithmType; set => _AlgorithmType = value; }
        public virtual int MaxIterations { get => _MaxIterations; set => _MaxIterations = value; }
        public virtual int ThreadCount { get => _ThreadCount; set => _ThreadCount = value; }
        public virtual double Magnification { get => _Magnification; set => _Magnification = value; }
        public virtual decimal offsetX { get => _offsetX; set => _offsetX = value; }
        public virtual decimal offsetY { get => _offsetY; set => _offsetY = value; }
        public virtual int Width { get => _Width; set => _Width = value; }
        public virtual int Height { get => _Height; set => _Height = value; }
        public virtual bool Gradual { get => _Gradual; set => _Gradual = value; }
        public virtual int[] MaxChunkSizes { get => _MaxChunkSizes; set => _MaxChunkSizes = value; }
    }
}
