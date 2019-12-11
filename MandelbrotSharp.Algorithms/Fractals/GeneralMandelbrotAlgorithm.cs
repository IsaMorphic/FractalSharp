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

namespace MandelbrotSharp.Algorithms.Fractals
{
    public class GeneralMandelbrotParams : EscapeTimeParams<double>
    {
        public Complex<double> Power { get; set; }

        public override IFractalParams Copy()
        {
            return new GeneralMandelbrotParams
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,

                EscapeRadius = EscapeRadius,
                Power = Power
            };
        }
    }
    public class GeneralMandelbrotAlgorithm :
        EscapeTimeAlgorithm<double, GeneralMandelbrotParams>
    {
        protected override Complex<double> DoIteration(Complex<double> z, Complex<double> c)
        {
            return CMath.Pow(z, Params.Power) + c;
        }
    }
}
