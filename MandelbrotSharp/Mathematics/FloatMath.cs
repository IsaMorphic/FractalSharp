using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Mathematics
{
    public class FloatMath : GenericMath<float>
    {
        public override float Add(float a, float b) { return a + b; }
        public override float Subtract(float a, float b) { return a - b; }

        public override float Multiply(float a, float b) { return a * b; }
        public override float Divide(float a, float b) { return a / b; }

        public override float Negate(float a) { return -a; }

        public override bool LessThan(float a, float b) { return a < b; }
        public override bool GreaterThan(float a, float b) { return a > b; }
        public override bool EqualTo(float a, float b) { return a == b; }

        public override float fromInt32(int a) { return a; }
        public override float fromDouble(double a) { return (float)a; }
        public override float fromBigDecimal(BigDecimal a) { return (float)a; }


        public override int toInt32(float a) { return (int)a; }
        public override double toDouble(float a) { return a; }
        public override BigDecimal toBigDecimal(float a) { return a; }
    }
}
