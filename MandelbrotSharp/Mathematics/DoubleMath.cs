using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Mathematics
{
    public class DoubleMath : GenericMath<double>
    {
        public override double Add(double a, double b) { return a + b; }
        public override double Subtract(double a, double b) { return a - b; }

        public override double Multiply(double a, double b) { return a * b; }
        public override double Divide(double a, double b) { return a / b; }

        public override double Negate(double a) { return -a; }

        public override bool LessThan(double a, double b) { return a < b; }
        public override bool GreaterThan(double a, double b) { return a > b; }
        public override bool EqualTo(double a, double b) { return a == b; }

        public override double fromInt32(int a) { return a; }
        public override double fromDouble(double a) { return a; }
        public override double fromBigDecimal(BigDecimal a) { return (double)(a.Truncate(18)); }


        public override int toInt32(double a) { return (int)a; }
        public override double toDouble(double a) { return a; }
        public override BigDecimal toBigDecimal(double a) { return a; }
    }
}
