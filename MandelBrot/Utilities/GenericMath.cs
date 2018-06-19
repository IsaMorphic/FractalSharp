using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quadruple;
namespace MandelBrot.Utilities
{
    interface GenericMath<T>
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
    }

    struct DoubleMath : GenericMath<Double>
    {
        public Double Add(Double a, Double b) { return a + b; }
        public Double Subtract(Double a, Double b) { return a - b; }
        public Double Multiply(Double a, Double b) { return a * b; }
        public Double Divide(Double a, Double b) { return a / b; }
    }

    struct QuadMath : GenericMath<Quad>
    {
        public Quad Add(Quad a, Quad b) { return a + b; }
        public Quad Subtract(Quad a, Quad b) { return a - b; }
        public Quad Multiply(Quad a, Quad b) { return a * b; }
        public Quad Divide(Quad a, Quad b) { return a / b; }
    }
}
