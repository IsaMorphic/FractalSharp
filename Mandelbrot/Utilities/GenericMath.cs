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
}
