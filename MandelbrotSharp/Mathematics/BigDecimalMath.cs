using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Mathematics
{
    public class BigDecimalMath : GenericMath<BigDecimal>
    {
        private static int MaxSignificantFigures = 300;

        public override BigDecimal Add(BigDecimal a, BigDecimal b) { return (a + b).Truncate(MaxSignificantFigures); }
        public override BigDecimal Subtract(BigDecimal a, BigDecimal b) { return (a - b).Truncate(MaxSignificantFigures); }

        public override BigDecimal Multiply(BigDecimal a, BigDecimal b) { return (a * b).Truncate(MaxSignificantFigures); }
        public override BigDecimal Divide(BigDecimal a, BigDecimal b) { return (a / b).Truncate(MaxSignificantFigures); }

        public override BigDecimal Negate(BigDecimal a) { return -a; }

        public override bool LessThan(BigDecimal a, BigDecimal b) { return a < b; }
        public override bool GreaterThan(BigDecimal a, BigDecimal b) { return a > b; }
        public override bool EqualTo(BigDecimal a, BigDecimal b) { return a == b; }

        public override BigDecimal fromInt32(int a) { return a; }
        public override BigDecimal fromDouble(double a) { return a; }
        public override BigDecimal fromBigDecimal(BigDecimal a) { return a; }


        public override int toInt32(BigDecimal a) { return (int)a; }
        public override double toDouble(BigDecimal a) { return (double)a; }
        public override BigDecimal toBigDecimal(BigDecimal a) { return a; }
    }
}
