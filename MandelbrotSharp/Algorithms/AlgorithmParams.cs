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
using System.Collections.Generic;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmParams
    {
        int MaxIterations { get; set; }
        INumber Magnification { get; set; }
        IComplex Location { get; set; }
    }

    public class AlgorithmParams<TNumber> : IAlgorithmParams where TNumber : struct
    {
        public int MaxIterations { get; set; }
        public Number<TNumber> Magnification { get; set; }
        public Complex<TNumber> Location { get; set; }

        int IAlgorithmParams.MaxIterations { get => MaxIterations; set => MaxIterations = value; }

        INumber IAlgorithmParams.Magnification { get => Magnification; set => Magnification = value.As<TNumber>(); }

        IComplex IAlgorithmParams.Location { get => Location; set => Location = value.As<TNumber>(); }
    }
}
