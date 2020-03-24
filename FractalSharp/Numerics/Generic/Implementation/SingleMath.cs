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
    public class SingleMath : IMath<float>
    {
        public float Add(float left, float right) => left + right;
        public float Subtract(float left, float right) => left - right;
        public float Multiply(float left, float right) => left * right;
        public float Divide(float left, float right) => left / right;

        public float Negate(float value) => -value;

        public bool Equal(float left, float right) => left == right;
        public bool NotEqual(float left, float right) => left != right;

        public bool LessThan(float left, float right) => left < right;
        public bool GreaterThan(float left, float right) => left > right;

        public bool LessThanOrEqual(float left, float right) => left <= right;
        public bool GreaterThanOrEqual(float left, float right) => left >= right;

        public double ToDouble(float value) => value;
        public float FromDouble(double value) => (float)value;

        public float Ln(float value) => (float)Math.Log(value);
        public float Exp(float value) => (float)Math.Exp(value);

        public float Pow(float x, float y) => (float)Math.Pow(x, y);

        public float Sqrt(float value) => (float)Math.Sqrt(value);

        public float Sin(float value) => (float)Math.Sin(value);
        public float Cos(float value) => (float)Math.Cos(value);
        public float Tan(float value) => (float)Math.Tan(value);

        public float Asin(float value) => (float)Math.Asin(value);
        public float Acos(float value) => (float)Math.Acos(value);
        public float Atan(float value) => (float)Math.Atan(value);

        public float Atan2(float y, float x) => (float)Math.Atan2(y, x);
    }
}
