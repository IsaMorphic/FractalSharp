using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public class AlgorithmParams
    {
        private CancellationToken _token;

        private int _maxIterations = 100;
        private BigDecimal _magnification = 1;
        private BigDecimal _offsetX = BigDecimal.Parse("-743643887037158704752191506114774E-33");
        private BigDecimal _offsetY =  BigDecimal.Parse("131825904205311970493132056385139E-33");

        public virtual CancellationToken Token { get => _token; set => _token = value; }

        public virtual BigDecimal Magnification { get => _magnification; set => _magnification = value; }
        public virtual BigDecimal offsetX { get => _offsetX; set => _offsetX = value; }
        public virtual BigDecimal offsetY { get => _offsetY; set => _offsetY = value; }
        public virtual int MaxIterations { get => _maxIterations; set => _maxIterations = value; }
    }
}
