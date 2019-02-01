using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Mathematics
{
    class FloatMath : IGenericMath<float>
    {
        public float Add(float a, float b) { return a + b; }
        public float Subtract(float a, float b) { return a - b; }

        public float Multiply(float a, float b) { return a * b; }
        public float Divide(float a, float b) { return a / b; }

        public float Negate(float a) { return -a; }

        public bool LessThan(float a, float b) { return a < b; }
        public bool GreaterThan(float a, float b) { return a > b; }
        public bool EqualTo(float a, float b) { return a == b; }

        public float fromInt32(int a) { return a; }
        public float fromDouble(double a) { return (float)a; }
        public float fromBigDecimal(BigDecimal a) { return (float)a; }


        public int toInt32(float a) { return (int)a; }
        public double toDouble(float a) { return a; }
        public BigDecimal toBigDecimal(float a) { return a; }
    }
}
