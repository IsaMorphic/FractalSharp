using MandelbrotSharp.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public class RenderSettings
    {
        private Type _algorithmType = typeof(TraditionalAlgorithmProvider<>);
        private Type _arithmeticType = typeof(double);

        private AlgorithmParams _algorithmParams = new AlgorithmParams();

        private int _threadCount = Environment.ProcessorCount;

        private int _width = 960;
        private int _height = 540;

        private bool _gradual = false;

        private int[] _maxChunkSizes = new int[12]
        {
            1, 1, 1, 1,
            1, 1, 1, 1,
            1, 1, 1, 1,
        };

        public virtual Type AlgorithmType { get => _algorithmType; set => _algorithmType = value; }
        public virtual Type ArithmeticType { get => _arithmeticType; set => _arithmeticType = value; }
        public virtual int ThreadCount { get => _threadCount; set => _threadCount = value; }
        public virtual int Width { get => _width; set => _width = value; }
        public virtual int Height { get => _height; set => _height = value; }
        public virtual bool Gradual { get => _gradual; set => _gradual = value; }
        public virtual int[] MaxChunkSizes { get => _maxChunkSizes; set => _maxChunkSizes = value; }


        public virtual BigDecimal Magnification { get => _algorithmParams.Magnification; set => _algorithmParams.Magnification = value; }
        public virtual BigDecimal offsetX { get => _algorithmParams.offsetX; set => _algorithmParams.offsetX = value; }
        public virtual BigDecimal offsetY { get => _algorithmParams.offsetY; set => _algorithmParams.offsetY = value; }
        public virtual int MaxIterations { get => _algorithmParams.MaxIterations; set => _algorithmParams.MaxIterations = value; }
    }
}
