/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MiscUtil;
using System;

namespace MandelbrotSharp.Numerics
{
    public interface INumber
    {
        Number<TOut> As<TOut>() where TOut : struct;
    }

    public struct Number<T> : INumber, IComparable<Number<T>>, IEquatable<Number<T>> where T : struct
    {
        public T Value { get; }

        public Number(T v)
        {
            Value = v;
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

        public static implicit operator T(Number<T> n)
        {
            return n.Value;
        }

        public static Number<T> operator +(Number<T> value)
        {
            return value;
        }

        public static Number<T> operator -(Number<T> value)
        {
            return value * -1;
        }

        public static Number<T> operator ++(Number<T> value)
        {
            return value + 1;
        }

        public static Number<T> operator --(Number<T> value)
        {
            return value - 1;
        }

        public static Number<T> operator +(Number<T> left, Number<T> right)
        {
            return new Number<T>(Operator.Add(left.Value, right.Value));
        }

        public static Number<T> operator -(Number<T> left, Number<T> right)
        {
            return new Number<T>(Operator.Subtract(left.Value, right.Value));
        }

        public static Number<T> operator *(Number<T> left, Number<T> right)
        {
            return new Number<T>(Operator.Multiply(left.Value, right.Value));
        }

        public static Number<T> operator /(Number<T> left, Number<T> right)
        {
            return new Number<T>(Operator.Divide(left.Value, right.Value));
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
            return new Number<TOut>(Operator.Convert<T, TOut>(Value));
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
            return Equals((Number<T>)obj);
        }

        public override int GetHashCode()
        {
            return 102981974 * Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
