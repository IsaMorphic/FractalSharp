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

namespace FractalSharp.Numerics
{
    public static class CMath
    {
        public static Number<double> Phase(Complex<double> z)
        {
            return Math.Atan2(z.Imag.Value, z.Real.Value);
        }

        public static Number<double> Abs(Complex<double> z)
        {
            Number<double> squ = z.Real * z.Real + z.Imag * z.Imag;
            return Math.Sqrt(squ.Value);
        }

        public static Complex<double> Exp(Complex<double> z)
        {
            Complex<double> r = Math.Exp(z.Real.Value);
            Complex<double> i = new Complex<double>(Math.Cos(z.Imag.Value), Math.Sin(z.Imag.Value));
            return r * i;
        }

        public static Complex<double> Log(Complex<double> z)
        {
            Number<double> log_r = Math.Log(Complex<double>.AbsSqu(z).Value) / 2.0;
            return new Complex<double>(log_r, Phase(z));
        }

        public static Complex<double> Pow(Complex<double> z, Complex<double> n)
        {
            if (z == Complex<double>.Zero)
                return Complex<double>.Zero;
            return Exp(Log(z) * n);
        }
    }
}
