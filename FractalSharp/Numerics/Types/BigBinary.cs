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
using System.Numerics;

namespace FractalSharp.Numerics.Types
{
    /// <summary>
    /// Arbitrary precision binary floating point number type
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Based on JcBernak's BigDecimal: https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d (licensed under public domain)
    /// </summary>
    public struct BigBinary
    : IComparable,
      IComparable<BigBinary>,
      IEquatable<BigBinary>
    {
        /// <summary>
        /// Sets the maximum precision of division operations.
        /// </summary>
        public static int Precision { get; } = 512;

        public BigInteger Mantissa { get; }
        public int Exponent { get; }

        public BigBinary(BigInteger mantissa, int exponent)
        {
            var val = Normalize(mantissa, exponent);

            Mantissa = val.Mantissa;
            Exponent = val.Exponent;
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public BigBinary Truncate(int precision)
        {
            var mantissa = Mantissa;
            var exponent = Exponent;
            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            if (NumberOfDigits(mantissa) > precision)
            {
                var diff = NumberOfDigits(mantissa) - precision;
                mantissa >>= diff;
                exponent += diff;
            }
            return new BigBinary(mantissa, exponent);
        }

        public BigBinary Floor()
        {
            return Truncate(NumberOfDigits(Mantissa) + Exponent);
        }

        private static (BigInteger Mantissa, int Exponent) Normalize(BigInteger mantissa, int exponent)
        {
            if (mantissa.IsZero)
            {
                exponent = 0;
            }
            else if (mantissa.IsEven)
            {
                BigInteger pow2 = ~(mantissa - 1) & mantissa;
                int exp = NumberOfDigits(pow2);
                mantissa >>= exp;
                exponent += exp;
            }

            return (mantissa, exponent);
        }

        private static int NumberOfDigits(BigInteger value)
        {
            return (int)Math.Ceiling(BigInteger.Log(value * value.Sign, 2));
        }

        #region Conversions

        public static implicit operator BigBinary(int value)
        {
            return new BigBinary(value, 0);
        }

        public static implicit operator BigBinary(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while ((double)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 2;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigBinary(mantissa, exponent);
        }

        public static explicit operator BigBinary(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (decimal)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 2;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigBinary(mantissa, exponent);
        }

        public static explicit operator BigBinary(BigDecimal value)
        {
            if (value.Exponent < 0)
            {
                BigInteger power = BigInteger.Pow(10, -value.Exponent);
                return new BigBinary(value.Mantissa, 0) / new BigBinary(power, 0);
            }
            else
            {
                return new BigBinary(value.Mantissa * BigInteger.Pow(10, value.Exponent), 0);
            }
        }

        public static explicit operator int(BigBinary value)
        {
            BigBinary truncated = value.Floor();
            return (int)(truncated.Mantissa << truncated.Exponent);
        }

        public static explicit operator float(BigBinary value)
        {
            return Convert.ToSingle((double)value);
        }

        public static explicit operator double(BigBinary value)
        {
            var truncated = value.Truncate(52);
            return (double)truncated.Mantissa * Math.Pow(2, truncated.Exponent);
        }

        public static explicit operator decimal(BigBinary value)
        {
            var truncated = value.Truncate(93);
            return (decimal)truncated.Mantissa * (decimal)Math.Pow(2, truncated.Exponent);
        }

        public static explicit operator BigDecimal(BigBinary value)
        {
            if (value.Exponent < 0)
            {
                BigInteger power = BigInteger.One << -value.Exponent;
                return new BigDecimal(value.Mantissa, 0) / new BigDecimal(power, 0);
            }
            else
            {
                return new BigDecimal(value.Mantissa << value.Exponent, 0);
            }
        }

        #endregion

        #region Operators

        public static BigBinary operator +(BigBinary value)
        {
            return value;
        }

        public static BigBinary operator -(BigBinary value)
        {
            return new BigBinary(-value.Mantissa, value.Exponent);
        }

        public static BigBinary operator +(BigBinary left, BigBinary right)
        {
            return Add(left, right);
        }

        public static BigBinary operator -(BigBinary left, BigBinary right)
        {
            return Add(left, -right);
        }

        public static BigBinary operator *(BigBinary left, BigBinary right)
        {
            return new BigBinary(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigBinary operator /(BigBinary dividend, BigBinary divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            var adjustedMantisa = dividend.Mantissa << exponentChange;
            return new BigBinary(adjustedMantisa / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        public static BigBinary operator %(BigBinary left, BigBinary right)
        {
            return left - right * (left / right).Floor();
        }

        public static bool operator ==(BigBinary left, BigBinary right)
        {
            return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
        }

        public static bool operator !=(BigBinary left, BigBinary right)
        {
            return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
        }

        public static bool operator <(BigBinary left, BigBinary right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) < right.Mantissa : left.Mantissa < AlignExponent(right, left);
        }

        public static bool operator >(BigBinary left, BigBinary right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) > right.Mantissa : left.Mantissa > AlignExponent(right, left);
        }

        public static bool operator <=(BigBinary left, BigBinary right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) <= right.Mantissa : left.Mantissa <= AlignExponent(right, left);
        }

        public static bool operator >=(BigBinary left, BigBinary right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) >= right.Mantissa : left.Mantissa >= AlignExponent(right, left);
        }

        /// <summary>
        /// Addition algorithm
        /// </summary>
        private static BigBinary Add(BigBinary left, BigBinary right)
        {
            return left.Exponent > right.Exponent
                ? new BigBinary(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigBinary(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigBinary value, BigBinary reference)
        {
            return value.Mantissa << (value.Exponent - reference.Exponent);
        }

        #endregion

        #region Additional mathematical functions

        public static BigBinary Exp(double exponent)
        {
            var tmp = (BigBinary)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * Math.Exp(exponent);
        }

        public static BigBinary Pow(double basis, double exponent)
        {
            var tmp = (BigBinary)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * Math.Pow(basis, exponent);
        }

        #endregion

        public override string ToString()
        {
            return ((BigDecimal)this).ToString();
        }

        public static BigBinary Parse(string s)
        {
            return (BigBinary)BigDecimal.Parse(s);
        }

        public bool Equals(BigBinary other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is BigBinary && Equals((BigBinary)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent;
            }
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is BigBinary))
            {
                throw new ArgumentException();
            }
            return CompareTo((BigBinary)obj);
        }

        public int CompareTo(BigBinary other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }
    }
}
