/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public interface IFractalParams : IAlgorithmParams
    {
        int MaxIterations { get; set; }
        INumber Magnification { get; set; }
        IComplex Location { get; set; }

        new IFractalParams Copy();
    }

    public abstract class FractalParams<TNumber> : IFractalParams where TNumber : struct
    {
        public int MaxIterations { get; set; }
        public Number<TNumber> Magnification { get; set; }
        public Complex<TNumber> Location { get; set; }

        int IFractalParams.MaxIterations { get => MaxIterations; set => MaxIterations = value; }
        INumber IFractalParams.Magnification { get => Magnification; set => Magnification = value.As<TNumber>(); }
        IComplex IFractalParams.Location { get => Location; set => Location = value.As<TNumber>(); }

        IAlgorithmParams IAlgorithmParams.Copy() => Copy();

        public abstract IFractalParams Copy();
    }
}
