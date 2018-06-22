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
        T fromInt32(Int32 a);
        T fromDouble(Double a);
        T fromQuadruple(Quadruple a);

        Int32 toInt32(T a);
        Double toDouble(T a);
        Quadruple toQuadruple(T a);
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
        public Double fromQuadruple(Quadruple a) { return (double)a; }

        public Int32 toInt32(Double a) { return (int)a; }
        public Double toDouble(Double a) { return a; }
        public Quadruple toQuadruple(Double a) { return a; }

    }

    struct QuadrupleMath : IGenericMath<Quadruple>
    {
        public Quadruple Add(Quadruple a, Quadruple b) { return a + b; }
        public Quadruple Subtract(Quadruple a, Quadruple b) { return a - b; }

        public Quadruple Multiply(Quadruple a, Quadruple b) { return a * b; }
        public Quadruple Divide(Quadruple a, Quadruple b) { return a / b; }

        public Quadruple Negate(Quadruple a) { return -a; }

        public bool LessThan(Quadruple a, Quadruple b) { return a < b; }
        public bool GreaterThan(Quadruple a, Quadruple b) { return a > b; }
        public bool EqualTo(Quadruple a, Quadruple b) { return a == b; }

        public Quadruple fromInt32(Int32 a) { return a; }
        public Quadruple fromDouble(Double a) { return a; }
        public Quadruple fromQuadruple(Quadruple a) { return a; }


        public Int32 toInt32(Quadruple a) { return (int)a; }
        public Double toDouble(Quadruple a) { return (double)a; }
        public Quadruple toQuadruple(Quadruple a) { return a; }
    }
}
