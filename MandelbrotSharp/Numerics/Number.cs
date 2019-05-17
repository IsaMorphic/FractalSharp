using MiscUtil;
using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Numerics
{
    public interface INumber
    {
        object Value { get; set; }
        Number<T> As<T>() where T : struct;
    }

    public struct Number<T> : INumber where T : struct
    {
        public T Value;

        object INumber.Value { get => Value; set => Value = (T)value; }

        public Number(T v)
        {
            Value = v;
        }

        public static implicit operator T(Number<T> n)
        {
            return n.Value;
        }

        public static implicit operator Number<T>(T n)
        {
            return new Number<T>(n);
        }

        public static implicit operator Number<T>(int n)
        {
            return From(n);
        }

        public static implicit operator Number<T>(double n)
        {
            return From(n);
        }

        public static implicit operator Number<T>(float n)
        {
            return From(n);
        }

        public static implicit operator Number<T>(decimal n)
        {
            return From(n);
        }

        public static explicit operator int(Number<T> n)
        {
            return n.As<int>();
        }

        public static explicit operator double(Number<T> n)
        {
            return n.As<double>();
        }

        public static explicit operator float(Number<T> n)
        {
            return n.As<float>();
        }

        public static explicit operator decimal(Number<T> n)
        {
            return n.As<decimal>();
        }

        public static Number<T> operator +(Number<T> left, Number<T> right)
        {
            return Operator.Add(left.Value, right.Value);
        }

        public static Number<T> operator -(Number<T> left, Number<T> right)
        {
            return Operator.Subtract(left.Value, right.Value);
        }

        public static Number<T> operator *(Number<T> left, Number<T> right)
        {
            return Operator.Multiply(left.Value, right.Value);
        }

        public static Number<T> operator /(Number<T> left, Number<T> right)
        {
            return Operator.Divide(left.Value, right.Value);
        }

        public static bool operator ==(Number<T> left, Number<T> right)
        {
            return Operator.Equal(left.Value, right.Value);
        }

        public static bool operator !=(Number<T> left, Number<T> right)
        {
            return Operator.NotEqual(left.Value, right.Value);
        }

        public static bool operator >(Number<T> left, Number<T> right)
        {
            return Operator.GreaterThan(left.Value, right.Value);
        }

        public static bool operator <(Number<T> left, Number<T> right)
        {
            return Operator.LessThan(left.Value, right.Value);
        }

        public static bool operator >=(Number<T> left, Number<T> right)
        {
            return Operator.GreaterThanOrEqual(left.Value, right.Value);
        }

        public static bool operator <=(Number<T> left, Number<T> right)
        {
            return Operator.LessThanOrEqual(left.Value, right.Value);
        }



        public static Number<T> From<TOut>(TOut n) where TOut : struct
        {
            return new Number<TOut>(n).As<T>();
        }

        public Number<TOut> As<TOut>() where TOut : struct
        {
            return Operator.Convert<T, TOut>(Value);
        }
    }
}
