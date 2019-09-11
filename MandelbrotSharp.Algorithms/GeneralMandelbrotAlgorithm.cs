using MandelbrotSharp.Data;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class GeneralMandelbrotParams : MTypeParams<double>
    {
        public Complex<double> Power { get; set; }

        public override IAlgorithmParams Copy()
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
        MTypeAlgorithm<double, GeneralMandelbrotParams>
    {
        protected override Complex<double> DoIteration(Complex<double> z, Complex<double> c)
        {
            return CMath.Pow(z, Params.Power) + c;
        }
    }
}
