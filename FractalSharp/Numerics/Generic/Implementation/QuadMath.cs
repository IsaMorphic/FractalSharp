using QuadrupleLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalSharp.Numerics.Generic.Implementation
{
    public class QuadMath : IMath<Float128>
    {
        public Float128 Add(Float128 left, Float128 right) => left + right;
        public Float128 Subtract(Float128 left, Float128 right) => left - right;
        public Float128 Multiply(Float128 left, Float128 right) => left * right;
        public Float128 Divide(Float128 left, Float128 right) => left / right;

        public Float128 Negate(Float128 value) => -value;

        public bool Equal(Float128 left, Float128 right) => left == right;
        public bool NotEqual(Float128 left, Float128 right) => left != right;

        public bool LessThan(Float128 left, Float128 right) => left < right;
        public bool GreaterThan(Float128 left, Float128 right) => left > right;

        public bool LessThanOrEqual(Float128 left, Float128 right) => left <= right;
        public bool GreaterThanOrEqual(Float128 left, Float128 right) => left >= right;

        public double ToDouble(Float128 value) => (double)value;
        public Float128 FromDouble(double value) => value;

        public Float128 Ln(Float128 value) => (Float128)Math.Log((double)value);
        public Float128 Exp(Float128 value) => (Float128)Math.Exp((double)value);

        public Float128 Pow(Float128 x, Float128 y) => (Float128)Math.Pow((double)x, (double)y);

        public Float128 Sqrt(Float128 value) => (Float128)Math.Sqrt((double)value);

        public Float128 Sin(Float128 value) => (Float128)Math.Sin((double)value);
        public Float128 Cos(Float128 value) => (Float128)Math.Cos((double)value);
        public Float128 Tan(Float128 value) => (Float128)Math.Tan((double)value);

        public Float128 Asin(Float128 value) => (Float128)Math.Asin((double)value);
        public Float128 Acos(Float128 value) => (Float128)Math.Acos((double)value);
        public Float128 Atan(Float128 value) => (Float128)Math.Atan((double)value);

        public Float128 Atan2(Float128 y, Float128 x) => (Float128)Math.Atan2((double)y, (double)x);
    }
}
