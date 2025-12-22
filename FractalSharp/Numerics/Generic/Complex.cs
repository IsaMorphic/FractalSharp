/*
 *  Copyright 2018-2026 Chosen Few Software
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FractalSharp.Numerics.Generic
{
    public partial struct Complex<TNumber> : INumber<Complex<TNumber>>
        where TNumber : struct, IFloatingPointIeee754<TNumber>
    {
        [GeneratedRegex(@"\<(\-?[0-9A-Fa-f]+\.?[0-9A-Fa-f]*),\s*(\-?[0-9A-Fa-f]+\.?[0-9A-Fa-f]*)\>")]
        private static partial Regex ParseGeneratedRegex();

        public static Complex<TNumber> Zero { get; } = new Complex<TNumber>(TNumber.Zero, TNumber.Zero);
        public static Complex<TNumber> One { get; } = new Complex<TNumber>(TNumber.One, TNumber.Zero);
        public static Complex<TNumber> ImaginaryOne { get; } = new Complex<TNumber>(TNumber.Zero, TNumber.One);

        public static Complex<TNumber> NaN { get; } = new Complex<TNumber>(TNumber.NaN, TNumber.NaN);
        public static Complex<TNumber> Infinity { get; } = new Complex<TNumber>(TNumber.PositiveInfinity, TNumber.PositiveInfinity);

        public static int Radix { get; } = TNumber.Radix;
        public static Complex<TNumber> AdditiveIdentity { get; } = Zero;
        public static Complex<TNumber> MultiplicativeIdentity { get; } = One;

        public TNumber Real { get; }
        public TNumber Imag { get; }

        public Complex(TNumber real, TNumber imag)
        {
            Real = real;
            Imag = imag;
        }

        public static implicit operator Complex<TNumber>(TNumber n)
        {
            return new Complex<TNumber>(n, TNumber.Zero);
        }

        public static Complex<TNumber> operator +(Complex<TNumber> value)
        {
            return value;
        }

        public static Complex<TNumber> operator -(Complex<TNumber> value)
        {
            return new Complex<TNumber>(-value.Real, -value.Imag);
        }

        public static Complex<TNumber> operator +(Complex<TNumber> left, Complex<TNumber> right)
        {
            return new Complex<TNumber>(left.Real + right.Real, left.Imag + right.Imag);
        }

        public static Complex<TNumber> operator -(Complex<TNumber> left, Complex<TNumber> right)
        {
            return new Complex<TNumber>(left.Real - right.Real, left.Imag - right.Imag);
        }

        public static Complex<TNumber> operator *(Complex<TNumber> left, Complex<TNumber> right)
        {
            // Three multiplication trick
            var ac = left.Real * right.Real;
            var bd = left.Imag * right.Imag;
            var prod = (left.Real + left.Imag) * (right.Real + right.Imag);
            return new Complex<TNumber>(ac - bd, prod - ac - bd);
        }

        public static Complex<TNumber> operator /(Complex<TNumber> left, Complex<TNumber> right)
        {
            Complex<TNumber> conjugate = new Complex<TNumber>(right.Real, -right.Imag);
            Complex<TNumber> numerator = left * conjugate;
            TNumber denominator = (right * conjugate).Real;
            return new Complex<TNumber>(numerator.Real / denominator, numerator.Imag / denominator);
        }

        public static bool operator ==(Complex<TNumber> left, Complex<TNumber> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Complex<TNumber> left, Complex<TNumber> right)
        {
            return !(left == right);
        }

        public static bool operator >(Complex<TNumber> left, Complex<TNumber> right)
        {
            if (left.Imag == TNumber.Zero && right.Imag == TNumber.Zero)
            {
                return left.Real > right.Real;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >=(Complex<TNumber> left, Complex<TNumber> right)
        {
            if (left.Imag == TNumber.Zero && right.Imag == TNumber.Zero)
            {
                return left.Real >= right.Real;
            }
            else
            {
                return false;
            }
        }

        public static bool operator <(Complex<TNumber> left, Complex<TNumber> right)
        {
            if (left.Imag == TNumber.Zero && right.Imag == TNumber.Zero)
            {
                return left.Real < right.Real;
            }
            else
            {
                return false;
            }
        }

        public static bool operator <=(Complex<TNumber> left, Complex<TNumber> right)
        {
            if (left.Imag == TNumber.Zero && right.Imag == TNumber.Zero)
            {
                return left.Real <= right.Real;
            }
            else
            {
                return false;
            }
        }

        public static Complex<TNumber> operator %(Complex<TNumber> left, Complex<TNumber> right)
        {
            return new Complex<TNumber>(TNumber.NaN, TNumber.NaN);
        }

        public static Complex<TNumber> operator --(Complex<TNumber> value)
        {
            return value - One;
        }

        public static Complex<TNumber> operator ++(Complex<TNumber> value)
        {
            return value + One;
        }

        public static TNumber AbsSqu(Complex<TNumber> value)
        {
            return value.Real * value.Real + value.Imag * value.Imag;
        }

        public bool Equals(Complex<TNumber> other)
        {
            if (IsNaN(this) || IsNaN(other))
            {
                return false;
            }
            else
            {
                return Real == other.Real && Imag == other.Imag;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Complex<TNumber> num && Equals(num);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Real, Imag);
        }

        public int CompareTo(object? obj)
        {
            switch (obj)
            {
                case Complex<TNumber> other:
                    return CompareTo(other);
                default:
                    throw new ArgumentException($"Provided value must be of type {nameof(Complex<TNumber>)}.", nameof(obj));
            }
        }

        public int CompareTo(Complex<TNumber> other)
        {
            if (IsRealNumber(this) && IsRealNumber(other))
            {
                return Real.CompareTo(other.Real);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Cannot compare complex and/or imaginary values.");
            }
        }

        public static Complex<TNumber> Abs(Complex<TNumber> value)
        {
            return TNumber.Sqrt(value.Real * value.Real + value.Imag * value.Imag);
        }

        public static bool IsCanonical(Complex<TNumber> value)
        {
            return TNumber.IsCanonical(value.Real) && TNumber.IsCanonical(value.Imag);
        }

        public static bool IsNormal(Complex<TNumber> value)
        {
            return value.Imag == TNumber.Zero && TNumber.IsNormal(value.Real);
        }

        public static bool IsSubnormal(Complex<TNumber> value)
        {
            return value.Imag == TNumber.Zero && TNumber.IsSubnormal(value.Real);
        }

        public static bool IsPositive(Complex<TNumber> value)
        {
            return value.Imag == TNumber.Zero && TNumber.IsPositive(value.Real);
        }

        public static bool IsNegative(Complex<TNumber> value)
        {
            return value.Imag == TNumber.Zero && TNumber.IsNegative(value.Real);
        }

        public static bool IsZero(Complex<TNumber> value)
        {
            return value == Zero;
        }

        public static bool IsFinite(Complex<TNumber> value)
        {
            return TNumber.IsFinite(value.Real) && TNumber.IsFinite(value.Imag);
        }

        public static bool IsInfinity(Complex<TNumber> value)
        {
            return TNumber.IsInfinity(value.Real) || TNumber.IsInfinity(value.Imag);
        }

        public static bool IsPositiveInfinity(Complex<TNumber> value)
        {
            if (value.Imag == TNumber.Zero)
            {
                return TNumber.IsPositiveInfinity(value.Real);
            }
            else
            {
                return false;
            }
        }

        public static bool IsNegativeInfinity(Complex<TNumber> value)
        {
            if (value.Imag == TNumber.Zero)
            {
                return TNumber.IsNegativeInfinity(value.Real);
            }
            else
            {
                return false;
            }
        }

        public static bool IsNaN(Complex<TNumber> value)
        {
            return TNumber.IsNaN(value.Real) || TNumber.IsNaN(value.Imag);
        }

        public static bool IsComplexNumber(Complex<TNumber> value)
        {
            return value.Real != TNumber.Zero && value.Imag != TNumber.Zero;
        }

        public static bool IsImaginaryNumber(Complex<TNumber> value)
        {
            return value.Real == TNumber.Zero;
        }

        public static bool IsRealNumber(Complex<TNumber> value)
        {
            return value.Imag == TNumber.Zero;
        }

        public static bool IsInteger(Complex<TNumber> value)
        {
            if (value.Imag == TNumber.Zero)
            {
                return TNumber.IsInteger(value.Real);
            }
            else
            {
                return false;
            }
        }

        public static bool IsEvenInteger(Complex<TNumber> value)
        {
            if (value.Imag == TNumber.Zero)
            {
                return TNumber.IsEvenInteger(value.Real);
            }
            else
            {
                return false;
            }
        }

        public static bool IsOddInteger(Complex<TNumber> value)
        {
            if (value.Imag == TNumber.Zero)
            {
                return TNumber.IsOddInteger(value.Real);
            }
            else
            {
                return false;
            }
        }

        public static Complex<TNumber> MaxMagnitude(Complex<TNumber> x, Complex<TNumber> y)
        {
            return AbsSqu(x) > AbsSqu(y) ? x : y;
        }

        public static Complex<TNumber> MaxMagnitudeNumber(Complex<TNumber> x, Complex<TNumber> y)
        {
            return IsNaN(y) || AbsSqu(x) > AbsSqu(y) ? x : y;
        }

        public static Complex<TNumber> MinMagnitude(Complex<TNumber> x, Complex<TNumber> y)
        {
            return AbsSqu(x) < AbsSqu(y) ? x : y;
        }

        public static Complex<TNumber> MinMagnitudeNumber(Complex<TNumber> x, Complex<TNumber> y)
        {
            return IsNaN(y) || AbsSqu(x) < AbsSqu(y) ? x : y;
        }

        public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<TNumber> result) where TOther : INumberBase<TOther>
        {
            TNumber real, imag;
            if (value is Complex z && (TNumber.TryConvertFromChecked(z.Real, out real) & TNumber.TryConvertFromChecked(z.Imaginary, out imag)))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else if (TNumber.TryConvertFromChecked(value, out real))
            {
                result = new Complex<TNumber>(real, TNumber.Zero);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<TNumber> result) where TOther : INumberBase<TOther>
        {
            TNumber real, imag;
            if (value is Complex z && (TNumber.TryConvertFromSaturating(z.Real, out real) & TNumber.TryConvertFromSaturating(z.Imaginary, out imag)))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else if (TNumber.TryConvertFromSaturating(value, out real))
            {
                result = new Complex<TNumber>(real, TNumber.Zero);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Complex<TNumber> result) where TOther : INumberBase<TOther>
        {
            TNumber real, imag;
            if (value is Complex z && (TNumber.TryConvertFromTruncating(z.Real, out real) & TNumber.TryConvertFromTruncating(z.Imaginary, out imag)))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else if (TNumber.TryConvertFromTruncating(value, out real))
            {
                result = new Complex<TNumber>(real, TNumber.Zero);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TryConvertToChecked<TOther>(Complex<TNumber> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            double real, imag;
            if (typeof(TOther) == typeof(Complex) && (TNumber.TryConvertToChecked(value.Real, out real) & TNumber.TryConvertToChecked(value.Imag, out imag)))
            {
                Complex z = new Complex(real, imag);
                result = Unsafe.As<Complex, TOther>(ref z);
                return true;
            }
            else if (IsRealNumber(value) && TNumber.TryConvertToChecked(value.Real, out result))
            {
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TryConvertToSaturating<TOther>(Complex<TNumber> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            double real, imag;
            if (typeof(TOther) == typeof(Complex) && (TNumber.TryConvertToSaturating(value.Real, out real) & TNumber.TryConvertToSaturating(value.Imag, out imag)))
            {
                Complex z = new Complex(real, imag);
                result = Unsafe.As<Complex, TOther>(ref z);
                return true;
            }
            else if (IsRealNumber(value) && TNumber.TryConvertToSaturating(value.Real, out result))
            {
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool TryConvertToTruncating<TOther>(Complex<TNumber> value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
        {
            double real, imag;
            if (typeof(TOther) == typeof(Complex) && (TNumber.TryConvertToTruncating(value.Real, out real) & TNumber.TryConvertToTruncating(value.Imag, out imag)))
            {
                Complex z = new Complex(real, imag);
                result = Unsafe.As<Complex, TOther>(ref z);
                return true;
            }
            else if (IsRealNumber(value) && TNumber.TryConvertToTruncating(value.Real, out result))
            {
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            charsWritten = 0;

            if (destination.Length == 0)
            {
                return false;
            }
            else if (destination.Length > charsWritten)
            {
                destination[charsWritten++] = '<';
            }

            int count0 = 0;
            bool result0 = destination.Length > charsWritten && Real.TryFormat(destination.Slice(charsWritten), out count0, format, provider);
            charsWritten += count0;

            if (!result0)
            {
                return false;
            }
            else if (destination.Length > charsWritten)
            {
                destination[charsWritten++] = ',';
            }

            int count1 = 0;
            bool result1 = destination.Length > charsWritten && Imag.TryFormat(destination.Slice(charsWritten), out count1, format, provider);
            charsWritten += count1;

            if (result1 && destination.Length > charsWritten)
            {
                destination[charsWritten++] = '>';
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"<{Real.ToString(format, formatProvider)},{Imag.ToString(format, formatProvider)}>";
        }

        public static Complex<TNumber> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            string str = new string(s);
            Match match = ParseGeneratedRegex().Match(str);
            TNumber real = TNumber.Parse(match.Groups[1].ValueSpan, style, provider);
            TNumber imag = TNumber.Parse(match.Groups[2].ValueSpan, style, provider);
            return new Complex<TNumber>(real, imag);
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<TNumber> result)
        {
            string str = new string(s);
            Match match = ParseGeneratedRegex().Match(str);
            if (match.Success &&
                TNumber.TryParse(match.Groups[1].ValueSpan, style, provider, out TNumber real) &&
                TNumber.TryParse(match.Groups[2].ValueSpan, style, provider, out TNumber imag))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Complex<TNumber> Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            Match match = ParseGeneratedRegex().Match(s);
            if (match.Success)
            {
                TNumber real = TNumber.Parse(match.Groups[1].ValueSpan, style, provider);
                TNumber imag = TNumber.Parse(match.Groups[2].ValueSpan, style, provider);
                return new Complex<TNumber>(real, imag);
            }
            else
            {
                throw new FormatException("Expected input with format: '<x,y>'");
            }
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<TNumber> result)
        {
            if (s is null)
            {
                result = default;
                return false;
            }

            Match match = ParseGeneratedRegex().Match(s);
            if (match.Success &&
                TNumber.TryParse(match.Groups[1].ValueSpan, style, provider, out TNumber real) &&
                TNumber.TryParse(match.Groups[2].ValueSpan, style, provider, out TNumber imag))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Complex<TNumber> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            string str = new string(s);
            Match match = ParseGeneratedRegex().Match(str);
            TNumber real = TNumber.Parse(match.Groups[1].ValueSpan, provider);
            TNumber imag = TNumber.Parse(match.Groups[2].ValueSpan, provider);
            return new Complex<TNumber>(real, imag);
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<TNumber> result)
        {
            string str = new string(s);
            Match match = ParseGeneratedRegex().Match(str);
            if (match.Success &&
                TNumber.TryParse(match.Groups[1].ValueSpan, provider, out TNumber real) &&
                TNumber.TryParse(match.Groups[2].ValueSpan, provider, out TNumber imag))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Complex<TNumber> Parse(string s, IFormatProvider? provider)
        {
            Match match = ParseGeneratedRegex().Match(s);
            if (match.Success)
            {
                TNumber real = TNumber.Parse(match.Groups[1].ValueSpan, provider);
                TNumber imag = TNumber.Parse(match.Groups[2].ValueSpan, provider);
                return new Complex<TNumber>(real, imag);
            }
            else
            {
                throw new FormatException("Expected input with format: '<x, y>'");
            }
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Complex<TNumber> result)
        {
            if (s is null)
            {
                result = default;
                return false;
            }

            Match match = ParseGeneratedRegex().Match(s);
            if (match.Success &&
                TNumber.TryParse(match.Groups[1].ValueSpan, provider, out TNumber real) &&
                TNumber.TryParse(match.Groups[2].ValueSpan, provider, out TNumber imag))
            {
                result = new Complex<TNumber>(real, imag);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}
