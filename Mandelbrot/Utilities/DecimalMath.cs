using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Utilities
{
    struct DecimalMath : IGenericMath<decimal>
    {
        public decimal Add(decimal a, decimal b) { return a + b; }
        public decimal Subtract(decimal a, decimal b) { return a - b; }

        public decimal Multiply(decimal a, decimal b) { return a * b; }
        public decimal Divide(decimal a, decimal b) { return a / b; }

        public decimal Negate(decimal a) { return -a; }

        public bool LessThan(decimal a, decimal b) { return a < b; }
        public bool GreaterThan(decimal a, decimal b) { return a > b; }
        public bool EqualTo(decimal a, decimal b) { return a == b; }

        public decimal fromInt32(int a) { return a; }
        public decimal fromDouble(double a) { return (decimal)a; }
        public decimal fromDecimal(decimal a) { return a; }


        public int toInt32(decimal a) { return (int)a; }
        public double toDouble(decimal a) { return (double)a; }
        public decimal toDecimal(decimal a) { return a; }
    }
}
