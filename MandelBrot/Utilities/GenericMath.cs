using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MandelBrot.Utilities
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
        T fromInt32(Int32 a);
        T fromDouble(Double a);
        T fromDecimal(Decimal a);

        Int32 toInt32(T a);
        Double toDouble(T a);
        Decimal toDecimal(T a);
    }

    struct DoubleMath : IGenericMath<Double>
    {
        public Double Add(Double a, Double b) { return a + b; }
        public Double Subtract(Double a, Double b) { return a - b; }

        public Double Multiply(Double a, Double b) { return a * b; }
        public Double Divide(Double a, Double b) { return a / b; }

        public Double Negate(Double a) { return -a; }

        public bool LessThan(Double a, Double b) { return a < b; }
        public bool GreaterThan(Double a, Double b) { return a > b; }
        public bool EqualTo(Double a, Double b) { return a == b; }

        public Double fromInt32(Int32 a) { return a; }
        public Double fromDouble(Double a) { return a; }
        public Double fromDecimal(Decimal a) { return (double)a; }

        public Int32 toInt32(Double a) { return (int)a; }
        public Double toDouble(Double a) { return a; }
        public Decimal toDecimal(Double a) { return (decimal)a; }

    }

    struct DecimalMath : IGenericMath<Decimal>
    {
        public Decimal Add(Decimal a, Decimal b) { return a + b; }
        public Decimal Subtract(Decimal a, Decimal b) { return a - b; }

        public Decimal Multiply(Decimal a, Decimal b) { return a * b; }
        public Decimal Divide(Decimal a, Decimal b) { return a / b; }

        public Decimal Negate(Decimal a) { return -a; }

        public bool LessThan(Decimal a, Decimal b) { return a < b; }
        public bool GreaterThan(Decimal a, Decimal b) { return a > b; }
        public bool EqualTo(Decimal a, Decimal b) { return a == b; }

        public Decimal fromInt32(Int32 a) { return a; }
        public Decimal fromDouble(Double a) { return (Decimal)a; }
        public Decimal fromDecimal(Decimal a) { return a; }

        public Int32 toInt32(Decimal a) { return (int)a; }
        public Double toDouble(Decimal a) { return (double)a; }
        public Decimal toDecimal(Decimal a) { return a; }

    }
}
