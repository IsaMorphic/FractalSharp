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

namespace FractalSharp.Numerics.Generic
{
    public interface IMath<T>
    {
        // Arithmetic
        T Add(T left, T right);
        T Subtract(T left, T right);
        T Multiply(T left, T right);
        T Divide(T left, T right);

        T Negate(T value);

        // Comparisons
        bool Equal(T left, T right);
        bool NotEqual(T left, T right);

        bool LessThan(T left, T right);
        bool GreaterThan(T left, T right);

        bool LessThanOrEqual(T left, T right);
        bool GreaterThanOrEqual(T left, T right);

        // Casts
        double ToDouble(T value);
        T FromDouble(double value);
    }


}
