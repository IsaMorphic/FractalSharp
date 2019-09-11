using System;
using System.Collections.Generic;
using System.Text;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class SquareMandelbrotParams<TNumber>
        : MTypeParams<TNumber>
        where TNumber : struct
    {
        public override IAlgorithmParams Copy()
        {
            return new SquareMandelbrotParams<TNumber>
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,

                EscapeRadius = EscapeRadius
            };
        }
    }

    public class SquareMandelbrotAlgorithm<TNumber> 
        : MTypeAlgorithm<TNumber, SquareMandelbrotParams<TNumber>>
        where TNumber : struct
    {
        protected override Complex<TNumber> DoIteration(Complex<TNumber> z, Complex<TNumber> c)
        {
            return z * z + c;
        }
    }
}
