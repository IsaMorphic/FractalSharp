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
    /// Arbitrary precision decimal.
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Original Source: https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d (licensed under public domain)
    /// Modified for better performance and code quality
    /// </summary>
    public struct BigDecimal
    : IComparable,
      IComparable<BigDecimal>,
      IEquatable<BigDecimal>
    {
        /// <summary>
        /// Sets the maximum precision of division operations.
        /// </summary>
        public static int Precision { get; } = 300;

        public BigInteger Mantissa { get; }
        public int Exponent { get; }

        public BigDecimal(BigInteger mantissa, int exponent)
        {
            var val = Normalize(mantissa, exponent);

            Mantissa = val.Mantissa;
            Exponent = val.Exponent;
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public BigDecimal Truncate(int precision)
        {
            var mantissa = Mantissa;
            var exponent = Exponent;
            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            if (NumberOfDigits(mantissa) > precision)
            {
                var diff = NumberOfDigits(mantissa) - precision;
                mantissa /= BigInteger.Pow(10, diff);
                exponent += diff;
            }
            return new BigDecimal(mantissa, exponent);
        }

        public BigDecimal Floor()
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
                BigInteger remainder = 0;
                while (remainder == 0)
                {
                    var shortened = BigInteger.DivRem(mantissa, 10, out remainder);
                    if (remainder == 0)
                    {
                        mantissa = shortened;
                        exponent++;
                    }
                }
            }
            return (mantissa, exponent);
        }

        private static int NumberOfDigits(BigInteger value)
        {
            return (int)Math.Ceiling(BigInteger.Log10(value * value.Sign));
        }

        #region Conversions

        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value, 0);
        }

        public static explicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static explicit operator int(BigDecimal value)
        {
            BigDecimal truncated = value.Floor();
            return (int)(truncated.Mantissa * BigInteger.Pow(10, truncated.Exponent));
        }

        public static explicit operator float(BigDecimal value)
        {
            return Convert.ToSingle((double)value);
        }

        public static explicit operator double(BigDecimal value)
        {
            var truncated = value.Truncate(16);
            return (double)truncated.Mantissa * Math.Pow(10, truncated.Exponent);
        }

        public static explicit operator decimal(BigDecimal value)
        {
            var truncated = value.Truncate(28);
            return (decimal)truncated.Mantissa * (decimal)Math.Pow(10, truncated.Exponent);
        }

        #endregion

        #region Operators

        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            return new BigDecimal(-value.Mantissa, value.Exponent);
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            var adjustedMantissa = dividend.Mantissa * BigInteger.Pow(10, exponentChange);
            return new BigDecimal(adjustedMantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            return left - right * (left / right).Floor();
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
        }

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) < right.Mantissa : left.Mantissa < AlignExponent(right, left);
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) > right.Mantissa : left.Mantissa > AlignExponent(right, left);
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) <= right.Mantissa : left.Mantissa <= AlignExponent(right, left);
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) >= right.Mantissa : left.Mantissa >= AlignExponent(right, left);
        }

        /// <summary>
        /// Addition Algorithm
        /// </summary>
        private static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent
                ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        }

        #endregion

        #region Additional mathematical functions

        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= (BigDecimal)Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * (BigDecimal)Math.Exp(exponent);
        }

        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= (BigDecimal)Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * (BigDecimal)Math.Pow(basis, exponent);
        }

        public static BigDecimal Abs(BigDecimal value)
        {
            return new BigDecimal(value.Mantissa * value.Mantissa.Sign, value.Exponent);
        }

        #endregion

        public override string ToString()
        {
            string digits = (Mantissa * Mantissa.Sign).ToString();
            string padded = "1";
            if (-Exponent >= digits.Length)
                padded = "0." + digits.PadLeft(digits.Length - (digits.Length + Exponent), '0');
            else if (Exponent < 0)
                padded = digits.Insert(digits.Length + Exponent, ".");
            else
                padded = digits.PadRight(digits.Length + Exponent, '0') + ".0";
            return ((Mantissa.Sign < 0) ? "-" : "") + padded;
        }

        public static BigDecimal Parse(string s)
        {
            var mantissa = BigInteger.Parse(s.Replace(".", ""));
            return new BigDecimal(mantissa, s.IndexOf('.') - s.Length + 1);
        }

        public bool Equals(BigDecimal other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is BigDecimal && Equals((BigDecimal)obj);
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
            if (ReferenceEquals(obj, null) || !(obj is BigDecimal))
            {
                throw new ArgumentException();
            }
            return CompareTo((BigDecimal)obj);
        }

        public int CompareTo(BigDecimal other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }
    }
}
