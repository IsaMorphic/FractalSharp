using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Utilities
{
    struct DoubleMath : IGenericMath<double>
    {
        public double Add(double a, double b) { return a + b; }
        public double Subtract(double a, double b) { return a - b; }

        public double Multiply(double a, double b) { return a * b; }
        public double Divide(double a, double b) { return a / b; }

        public double Negate(double a) { return -a; }

        public bool LessThan(double a, double b) { return a < b; }
        public bool GreaterThan(double a, double b) { return a > b; }
        public bool EqualTo(double a, double b) { return a == b; }

        public double fromInt32(int a) { return a; }
        public double fromDouble(double a) { return a; }
        public double fromDecimal(decimal a) { return (double)a; }


        public int toInt32(double a) { return (int)a; }
        public double toDouble(double a) { return a; }
        public decimal toDecimal(double a) { return (decimal)a; }
    }
}
