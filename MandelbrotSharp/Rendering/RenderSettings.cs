using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
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
        private Type _pixelColoratorType = typeof(PixelColorator);

        private AlgorithmParams _algorithmParams = new AlgorithmParams();

        private int _threadCount = Environment.ProcessorCount;

        private int _width = 960;
        private int _height = 540;

        private RgbaValue[] _palette;

        public virtual AlgorithmParams AlgorithmParams { get => _algorithmParams; set => _algorithmParams = value; }
        public virtual Type AlgorithmType { get => _algorithmType; set => _algorithmType = value; }
        public virtual Type ArithmeticType { get => _arithmeticType; set => _arithmeticType = value; }
        public virtual Type PixelColoratorType { get => _pixelColoratorType; set => _pixelColoratorType = value; }
        public virtual RgbaValue[] Palette { get => _palette; set => _palette = value; }
        public virtual int ThreadCount { get => _threadCount; set => _threadCount = value; }
        public virtual int Width { get => _width; set => _width = value; }
        public virtual int Height { get => _height; set => _height = value; }


        public virtual BigDecimal Magnification { get => _algorithmParams.Magnification; set => _algorithmParams.Magnification = value; }
        public virtual BigDecimal offsetX { get => _algorithmParams.offsetX; set => _algorithmParams.offsetX = value; }
        public virtual BigDecimal offsetY { get => _algorithmParams.offsetY; set => _algorithmParams.offsetY = value; }
        public virtual int MaxIterations { get => _algorithmParams.MaxIterations; set => _algorithmParams.MaxIterations = value; }
        public virtual Dictionary<string, object> ExtraParams { get => _algorithmParams.ExtraParams; set => _algorithmParams.ExtraParams = value; }
    }
}
