using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quadruple;
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
        T Pow(T a, T b);

        // Comparisions
        bool LessThan(T a, T b);
        bool GreaterThan(T a, T b);
        bool EqualTo(T a, T b);

        // Specific Casts
        T fromInt32(Int32 a);
        T fromDouble(Double a);
        T fromQuad(Quad a);

        Int32 toInt32(T a);
        Double toDouble(T a);
        Quad toQuad(T a);
    }

    struct DoubleMath : IGenericMath<Double>
    {
        public Double Add(Double a, Double b) { return a + b; }
        public Double Subtract(Double a, Double b) { return a - b; }

        public Double Multiply(Double a, Double b) { return a * b; }
        public Double Divide(Double a, Double b) { return a / b; }

        public Double Negate(Double a) { return -a; }
        public Double Pow(Double a, Double b) { return Math.Pow(a, b); }

        public bool LessThan(Double a, Double b) { return a < b; }
        public bool GreaterThan(Double a, Double b) { return a > b; }
        public bool EqualTo(Double a, Double b) { return a == b; }

        public Double fromInt32(Int32 a) { return a; }
        public Double fromDouble(Double a) { return a; }
        public Double fromQuad(Quad a) { return (double)a; }

        public Int32 toInt32(Double a) { return (int)a; }
        public Double toDouble(Double a) { return a; }
        public Quad toQuad(Double a) { return a; }

    }

    struct QuadMath : IGenericMath<Quad>
    {
        public Quad Add(Quad a, Quad b) { return a + b; }
        public Quad Subtract(Quad a, Quad b) { return a - b; }

        public Quad Multiply(Quad a, Quad b) { return a * b; }
        public Quad Divide(Quad a, Quad b) { return a / b; }

        public Quad Negate(Quad a) { return -a; }
        public Quad Pow(Quad a, Quad b) { return Quad.Pow(a, (double)b); }

        public bool LessThan(Quad a, Quad b) { return a < b; }
        public bool GreaterThan(Quad a, Quad b) { return a > b; }
        public bool EqualTo(Quad a, Quad b) { return a == b; }

        public Quad fromInt32(Int32 a) { return a; }
        public Quad fromDouble(Double a) { return a; }
        public Quad fromQuad(Quad a) { return a; }

        public Int32 toInt32(Quad a) { return (int)a; }
        public Double toDouble(Quad a) { return (double)a; }
        public Quad toQuad(Quad a) { return a; }
    }
}
