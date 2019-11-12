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

namespace MandelbrotSharp.Algorithms
{
    public class PowerJuliaParams : EscapeTimeParams<double>
    {
        public Complex<double> Coordinates { get; set; }

        public override IAlgorithmParams Copy()
        {
            return new PowerJuliaParams
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,

                EscapeRadius = EscapeRadius,
                Coordinates = Coordinates
            };
        }
    }

    public class PowerJuliaAlgorithm : JuliaAlgorithm<double, PowerJuliaParams>
    {
        protected override Complex<double> DoIteration(Complex<double> z)
        {
            return CMath.Pow(Params.Coordinates, z);
        }
    }
}
