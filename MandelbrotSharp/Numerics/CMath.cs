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
