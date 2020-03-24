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

namespace FractalSharp.Numerics.Generic.Implementation
{
    public class DecimalMath : IMath<decimal>
    {
        public decimal Add(decimal left, decimal right) => left + right;
        public decimal Subtract(decimal left, decimal right) => left - right;
        public decimal Multiply(decimal left, decimal right) => left * right;
        public decimal Divide(decimal left, decimal right) => left / right;

        public decimal Negate(decimal value) => -value;

        public bool Equal(decimal left, decimal right) => left == right;
        public bool NotEqual(decimal left, decimal right) => left != right;

        public bool LessThan(decimal left, decimal right) => left < right;
        public bool GreaterThan(decimal left, decimal right) => left > right;

        public bool LessThanOrEqual(decimal left, decimal right) => left <= right;
        public bool GreaterThanOrEqual(decimal left, decimal right) => left >= right;

        public double ToDouble(decimal value) => (double)value;
        public decimal FromDouble(double value) => (decimal)value;

        public decimal Ln(decimal value) => throw new System.NotImplementedException();
        public decimal Exp(decimal value) => throw new System.NotImplementedException();

        public decimal Pow(decimal x, decimal y) => throw new System.NotImplementedException();

        public decimal Sqrt(decimal value) => throw new System.NotImplementedException();

        public decimal Sin(decimal value) => throw new System.NotImplementedException();
        public decimal Cos(decimal value) => throw new System.NotImplementedException();
        public decimal Tan(decimal value) => throw new System.NotImplementedException();

        public decimal Asin(decimal value) => throw new System.NotImplementedException();
        public decimal Acos(decimal value) => throw new System.NotImplementedException();
        public decimal Atan(decimal value) => throw new System.NotImplementedException();

        public decimal Atan2(decimal y, decimal x) => throw new System.NotImplementedException();
    }
}
