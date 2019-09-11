using System;
using System.Collections.Generic;
using System.Text;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class PerpendicularMandelbrotAlgorithm<TNumber> 
        : SquareMandelbrotAlgorithm<TNumber>
        where TNumber : struct
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z, Complex<TNumber> c)
        {
            var real = z.Real < Number<TNumber>.Zero ? -z.Real : z.Real;
            var imag = new Complex<TNumber>(Number<TNumber>.Zero, z.Imag);
            var y = real - imag;
            return y * y + c;
        }
    }
}
