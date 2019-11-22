using System;
using System.Collections.Generic;
using System.Text;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms.Fractals
{
    public class GlitchMandelbrotAlgorithm<TNumber> :
        EscapeTimeAlgorithm<TNumber, EscapeTimeParams<TNumber>>
        where TNumber : struct
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z, Complex<TNumber> c)
        {
            Number<TNumber> real = z.Real;
            Number<TNumber> imag = z.Imag;
            real = real * real - imag * imag + c.Real;
            imag = Number<TNumber>.Two * real * imag + c.Imag;
            return new Complex<TNumber>(real, imag);
        }
    }
}
