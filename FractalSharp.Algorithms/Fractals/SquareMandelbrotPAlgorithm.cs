using FractalSharp.Numerics.Generic;
using System.Collections.Generic;
using System.Threading;

namespace FractalSharp.Algorithms.Fractals
{
    public class PerturbationParams<TNumber> : EscapeTimeParams<TNumber>
        where TNumber : struct
    {
        public Complex<TNumber> ReferencePoint { get; set; }

        public PerturbationParams() : base()
        {
            ReferencePoint = Location;
        }

        public override IFractalParams Copy()
        {
            return new PerturbationParams<TNumber>
            {
                Location = Location,
                Magnification = Magnification,
                MaxIterations = MaxIterations,
                EscapeRadius = EscapeRadius,

                ReferencePoint = ReferencePoint
            };
        }
    }

    public class SquareMandelbrotPAlgorithm<TNumber> : SquareMandelbrotAlgorithm<TNumber>
        where TNumber : struct
    {
        private readonly List<Complex<double>> ReferenceOrbit = new List<Complex<double>> { 0.0 };

        private new PerturbationParams<TNumber> Params => base.Params as PerturbationParams<TNumber>;

        public override Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio)
        {
            Number<TNumber> xScale = aspectRatio * Number<TNumber>.Two / Params.Magnification;

            Number<TNumber> xMin = -xScale + Params.Location.Real - Params.ReferencePoint.Real;
            Number<TNumber> xMax = xScale + Params.Location.Real - Params.ReferencePoint.Real;

            Number<TNumber> yScale = Number<TNumber>.Two / Params.Magnification;

            Number<TNumber> yMin = yScale + Params.Location.Imag - Params.ReferencePoint.Imag;
            Number<TNumber> yMax = -yScale + Params.Location.Imag - Params.ReferencePoint.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        protected override bool Initialize(CancellationToken cancellationToken)
        {
            var z = Complex<TNumber>.Zero;
            var c = Params.ReferencePoint;

            int i = 0;
            while (Complex<TNumber>.AbsSqu(z) < Params.EscapeRadius && i++ < Params.MaxIterations)
            {
                var z_n = base.DoIteration(z, c);
                ReferenceOrbit.Add((z = z_n).ToDouble());

                cancellationToken.ThrowIfCancellationRequested();
            }

            return true;
        }

        public override PointData Run(Complex<TNumber> mappedPoint)
        {
            // Initialize some variables..
            Complex<double> epsilon = 0.0;
            Complex<double> delta = mappedPoint.ToDouble();

            // Initialize our iteration count.
            int iter = 0;

            // Mandelbrot algorithm
            while (iter < ReferenceOrbit.Count - 1 && Complex<double>.AbsSqu(ReferenceOrbit[iter] + epsilon) < Params.EscapeRadius.ToDouble())
            {
                epsilon = DoIteration(epsilon, delta, iter);
                iter++;
            }

            if (Complex<double>.AbsSqu(ReferenceOrbit[iter] + epsilon) > Params.EscapeRadius.ToDouble())
            {
                return new PointData(ReferenceOrbit[iter] + epsilon, iter, iter < Params.MaxIterations ? PointClass.Outer : PointClass.Inner);
            }
            else
            {
                var lastOrbit = ReferenceOrbit[iter] + epsilon;
                Complex<TNumber> prevOutput = new Complex<TNumber>(
                    Number<TNumber>.FromDouble(lastOrbit.Real),
                    Number<TNumber>.FromDouble(lastOrbit.Imag));

                while (Complex<TNumber>.AbsSqu(prevOutput) < Params.EscapeRadius && iter < Params.MaxIterations)
                {
                    prevOutput = DoIteration(prevOutput, mappedPoint + Params.ReferencePoint);
                    iter++;
                }

                return new PointData(prevOutput.ToDouble(), iter, iter < Params.MaxIterations ? PointClass.Outer : PointClass.Inner);
            }
        }

        private Complex<double> DoIteration(Complex<double> epsilon, Complex<double> delta, int iter)
        {
            return 2.0 * ReferenceOrbit[iter] * epsilon + epsilon * epsilon + delta;
        }
    }
}
