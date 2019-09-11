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
using System;

namespace MandelbrotSharp.Numerics
{
    public class CMath
    {
        public static Complex<double> Exp(Complex<double> z)
        {
            Complex<double> r = Math.Exp(z.Real.Value);
            Complex<double> i = new Complex<double>(Math.Cos(z.Imag.Value), Math.Sin(z.Imag.Value));
            return r * i;
        }

        public static Complex<double> Log(Complex<double> z)
        {
            Number<double> log_r = Math.Log(z.MagnitudeSqu.Value) / 2.0;
            return new Complex<double>(log_r, z.Phase);
        }

        public static Complex<double> Pow(Complex<double> z, Complex<double> n)
        {
            if (z == Complex<double>.Zero)
                return Complex<double>.Zero;
            return Exp(Log(z) * n);
        }
    }
}
