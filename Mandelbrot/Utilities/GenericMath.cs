using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Utilities
{
    interface IGenericMath<T>
    {
        // Basic Operations
        T Add(T a, T b);
        T Subtract(T a, T b);

        T Multiply(T a, T b);
        T Divide(T a, T b);

        T Negate(T a);

        // Comparisions
        bool LessThan(T a, T b);
        bool GreaterThan(T a, T b);
        bool EqualTo(T a, T b);

        // Specific Casts
        T fromInt32(int a);
        T fromDouble(double a);
        T fromDecimal(decimal a);

        int toInt32(T a);
        double toDouble(T a);
        decimal toDecimal(T a);
    }

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
