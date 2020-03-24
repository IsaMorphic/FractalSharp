/*
 *  Copyright 2018-2020 Chosen Few Software
 *  This file is part of FractalSharp.
 *
 *  FractalSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  FractalSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with FractalSharp.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace FractalSharp.Numerics.Generic
{
    public interface INumber
    {
        double ToDouble();
    }

    public struct Number<T> : INumber, IComparable<Number<T>>, IEquatable<Number<T>> where T : struct
    {
        #region Static members

        // Math interface
        private static IMath<T> Math { get; }

        // Constants
        public static Number<T> Zero { get; }
        public static Number<T> One { get; }
        public static Number<T> Two { get; }

        static Number()
        {
            Math = (MathFactory.Instance as
                IMathFactory<T>).Create();

            Zero = Math.FromDouble(0.0);
            One = Math.FromDouble(1.0);
            Two = Math.FromDouble(2.0);
        }

        #endregion

        public T Value { get; }

        public Number(T v)
        {
            Value = v;
        }

        public static implicit operator Number<T>(T n)
        {
            return new Number<T>(n);
        }

        public static Number<T> operator +(Number<T> value)
        {
            return value;
        }

        public static Number<T> operator -(Number<T> value)
        {
            return new Number<T>(Math.Negate(value.Value));
        }

        public static Number<T> operator +(Number<T> left, Number<T> right)
        {
            return new Number<T>(Math.Add(left.Value, right.Value));
        }

        public static Number<T> operator -(Number<T> left, Number<T> right)
        {
            return new Number<T>(Math.Subtract(left.Value, right.Value));
        }

        public static Number<T> operator *(Number<T> left, Number<T> right)
        {
            return new Number<T>(Math.Multiply(left.Value, right.Value));
        }

        public static Number<T> operator /(Number<T> left, Number<T> right)
        {
            return new Number<T>(Math.Divide(left.Value, right.Value));
        }

        public static bool operator ==(Number<T> left, Number<T> right)
        {
            return Math.Equal(left.Value, right.Value);
        }

        public static bool operator !=(Number<T> left, Number<T> right)
        {
            return Math.NotEqual(left.Value, right.Value);
        }

        public static bool operator >(Number<T> left, Number<T> right)
        {
            return Math.GreaterThan(left.Value, right.Value);
        }

        public static bool operator <(Number<T> left, Number<T> right)
        {
            return Math.LessThan(left.Value, right.Value);
        }

        public static bool operator >=(Number<T> left, Number<T> right)
        {
            return Math.GreaterThanOrEqual(left.Value, right.Value);
        }

        public static bool operator <=(Number<T> left, Number<T> right)
        {
            return Math.LessThanOrEqual(left.Value, right.Value);
        }

        public static Number<T> Abs(Number<T> value)
        {
            return (value > Zero) ? value : -value;
        }

        public double ToDouble()
        {
            return Math.ToDouble(Value);
        }

        public static Number<T> FromDouble(Number<double> value)
        {
            return new Number<T>(Math.FromDouble(value.Value));
        }

        public int CompareTo(Number<T> other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }

        public bool Equals(Number<T> other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            return obj is Number<T> && Equals((Number<T>)obj);
        }

        public override int GetHashCode()
        {
            return 102981974 + Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
