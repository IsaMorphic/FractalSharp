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

using FractalSharp.Numerics.Generic.Implementation;
using System;

namespace FractalSharp.Numerics.Generic
{
    public interface IMathFactory<T>
    {
        IMath<T> Create();
    }

    public partial class MathFactory : IMathFactory<float>, IMathFactory<double>, IMathFactory<decimal>
    {
        public static MathFactory Instance { get; } = new MathFactory();

        IMath<float> IMathFactory<float>.Create()
        {
            return new SingleMath();
        }

        IMath<double> IMathFactory<double>.Create()
        {
            return new DoubleMath();
        }

        IMath<decimal> IMathFactory<decimal>.Create()
        {
            throw new NotImplementedException();
        }
    }
}
