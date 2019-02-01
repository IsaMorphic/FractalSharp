using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Mathematics
{
    class BigDecimalMath : IGenericMath<BigDecimal>
    {
        private static int MaxSignificantFigures = 200;
        private static int MaxDoubleConversionFigures = 18;

        public BigDecimal Add(BigDecimal a, BigDecimal b) { return (a + b).Truncate(MaxSignificantFigures); }
        public BigDecimal Subtract(BigDecimal a, BigDecimal b) { return (a - b).Truncate(MaxSignificantFigures); }

        public BigDecimal Multiply(BigDecimal a, BigDecimal b) { return (a * b).Truncate(MaxSignificantFigures); }
        public BigDecimal Divide(BigDecimal a, BigDecimal b) { return (a / b).Truncate(MaxSignificantFigures); }

        public BigDecimal Negate(BigDecimal a) { return -a; }

        public bool LessThan(BigDecimal a, BigDecimal b) { return a < b; }
        public bool GreaterThan(BigDecimal a, BigDecimal b) { return a > b; }
        public bool EqualTo(BigDecimal a, BigDecimal b) { return a == b; }

        public BigDecimal fromInt32(int a) { return a; }
        public BigDecimal fromDouble(double a) { return a; }
        public BigDecimal fromBigDecimal(BigDecimal a) { return a; }


        public int toInt32(BigDecimal a) { return (int)a; }
        public double toDouble(BigDecimal a) { return (double)(a.Truncate(MaxDoubleConversionFigures)); }
        public BigDecimal toBigDecimal(BigDecimal a) { return a; }
    }
}
