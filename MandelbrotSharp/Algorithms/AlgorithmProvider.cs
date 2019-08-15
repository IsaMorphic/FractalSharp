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
using MandelbrotSharp.Numerics;
using System.Threading;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider<TNumber> where TNumber : struct
    {
        void Initialize(IAlgorithmParams @params, CancellationToken token);
        PointData Run(Complex<TNumber> point);
    }

    public abstract class AlgorithmProvider<TNumber, TParam> : IAlgorithmProvider<TNumber>
        where TParam : AlgorithmParams<TNumber> 
        where TNumber : struct
    {
        protected TParam Params { get; private set; }

        public void Initialize(IAlgorithmParams @params, CancellationToken token)
        {
            Params = @params as TParam;
            Initialize(token);
        }

        public abstract PointData Run(Complex<TNumber> point);

        protected virtual void Initialize(CancellationToken token) { }
    }
}
