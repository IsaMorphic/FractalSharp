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

namespace FractalSharp.Numerics.Generic.Implementation
{
    public class DoubleMath : IMath<double>
    {
        public double Add(double left, double right) => left + right;
        public double Subtract(double left, double right) => left - right;
        public double Multiply(double left, double right) => left * right;
        public double Divide(double left, double right) => left / right;

        public double Negate(double value) => -value;

        public bool Equal(double left, double right) => left == right;
        public bool NotEqual(double left, double right) => left != right;

        public bool LessThan(double left, double right) => left < right;
        public bool GreaterThan(double left, double right) => left > right;

        public bool LessThanOrEqual(double left, double right) => left <= right;
        public bool GreaterThanOrEqual(double left, double right) => left >= right;

        public double ToDouble(double value) => value;
        public double FromDouble(double value) => value;

        public double Ln(double value) => Math.Log(value);
        public double Exp(double value) => Math.Exp(value);

        public double Pow(double x, double y) => Math.Pow(x, y);

        public double Sqrt(double value) => Math.Sqrt(value);

        public double Sin(double value) => Math.Sin(value);
        public double Cos(double value) => Math.Cos(value);
        public double Tan(double value) => Math.Tan(value);

        public double Asin(double value) => Math.Asin(value);
        public double Acos(double value) => Math.Acos(value);
        public double Atan(double value) => Math.Atan(value);

        public double Atan2(double y, double x) => Math.Atan2(y, x);
    }
}
