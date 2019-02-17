using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Mathematics
{
    public abstract class GenericMath<T>
    {
        // Basic Operations
        public abstract T Add(T a, T b);
        public abstract T Subtract(T a, T b);

        public abstract T Multiply(T a, T b);
        public abstract T Divide(T a, T b);

        public abstract T Negate(T a);

        // Comparisions
        public abstract bool LessThan(T a, T b);
        public abstract bool GreaterThan(T a, T b);
        public abstract bool EqualTo(T a, T b);

        // Specific Casts
        public abstract T fromInt32(int a);
        public abstract T fromDouble(double a);
        public abstract T fromBigDecimal(BigDecimal a);

        public abstract int toInt32(T a);
        public abstract double toDouble(T a);
        public abstract BigDecimal toBigDecimal(T a);
    }
}
