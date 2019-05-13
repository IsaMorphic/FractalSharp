using MandelbrotSharp.Imaging;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public class PerturbationAlgorithmProvider<T> : AlgorithmProvider<T>
    {
        private List<Complex> X, TwoX, A, B, C;
        private List<Complex[]>[] ProbePoints;
        private T newReferenceX, newReferenceY;
        private T referenceX, referenceY;

        private T Zero, Four;

        private int SkippedIterations;
        private int MostIterations;
        private int PreviousMaxIterations;

        [Parameter(DefaultValue = true)]
        public bool ShouldUseSeriesApproximation;

        [Parameter(DefaultValue = 20)]
        public int NumProbePoints;

        public PerturbationAlgorithmProvider()
        {
            // Constants
            Zero = Operator.Convert<int, T>(0);
            Four = Operator.Convert<int, T>(4);

            // Initialize Lists
            A = new List<Complex>();
            B = new List<Complex>();
            C = new List<Complex>();
            X = new List<Complex>();
            TwoX = new List<Complex>();            
        }

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points

        protected override void OnParamsUpdated()
        {
            ProbePoints = new List<Complex[]>[NumProbePoints];

            A.Clear();
            B.Clear();
            C.Clear();
            X.Clear();
            TwoX.Clear();

            MostIterations = 0;

            if (PreviousMaxIterations != Params.MaxIterations)
            {
                newReferenceX = Operator.Convert<BigDecimal, T>(Params.offsetX);
                newReferenceY = Operator.Convert<BigDecimal, T>(Params.offsetY);
            }

            referenceX = newReferenceX;
            referenceY = newReferenceY;

            Task.Run(() =>
            {
                Random random = new Random();
                for (int i = 0; i < ProbePoints.Length; i++)
                {
                    ProbePoints[i] = new List<Complex[]>();
                    var point = new Complex((double)((random.NextDouble() * 4 - 2) / Params.Magnification + Params.offsetX), (double)((random.NextDouble() * 4 - 2) / Params.Magnification + Params.offsetY));
                    ProbePoints[i].Add(new Complex[3] { point, point * point, point * point * point });
                }

                A.Add(new Complex(1, 0));
                B.Add(new Complex(0, 0));
                C.Add(new Complex(0, 0));

                IterateReferencePoint();

                if (ShouldUseSeriesApproximation)
                    ApproximateSeries();

                PreviousMaxIterations = Params.MaxIterations;
                base.OnParamsUpdated();
            });
        }

        private double MagnitudeSquared(Complex a)
        {
            return a.Real * a.Real + a.Imaginary * a.Imaginary;
        }

        public void IterateReferencePoint()
        {
            T xn_r, x0_r = xn_r = referenceX;
            T xn_i, x0_i = xn_i = referenceY;

            for (int i = 0; i < Params.MaxIterations; i++)
            {
                Params.Token.ThrowIfCancellationRequested();
                // pre multiply by two
                T real = Operator.Add(xn_r, xn_r);
                T imag = Operator.Add(xn_i, xn_i);

                T xn_r2 = Operator.Multiply(xn_r, xn_r);
                T xn_i2 = Operator.Multiply(xn_i, xn_i);

                Complex c = new Complex(Operator.Convert<T, double>(xn_r), Operator.Convert<T, double>(xn_i));
                Complex two_c = new Complex(Operator.Convert<T, double>(real), Operator.Convert<T, double>(imag));

                X.Add(c);
                TwoX.Add(two_c);
                // calculate next iteration, remember real = 2 * xn_r
                if (Operator.GreaterThan(Operator.Add(xn_r2, xn_i2), Four))
                    break;


                xn_r = Operator.Add(Operator.Subtract(xn_r2, xn_i2), x0_r);
                xn_i = Operator.Add(Operator.Multiply(real, xn_i), x0_i);
            }
        }

        private void IterateProbePoints(int n)
        {
            foreach (var P in ProbePoints)
            {
                var d0 = P[0][0];
                var dn = P[n - 1][0];
                dn *= TwoX[n] + dn;
                // dn += d0
                dn += d0;
                P.Add(new Complex[] { dn });
            }
        }

        private void IterateA(int n)
        {
            A.Add(2 * X[n - 1] * A[n - 1] + 1);
        }

        private void IterateB(int n)
        {
            B.Add(2 * X[n - 1] * B[n - 1] + A[n - 1] * A[n - 1]);
        }

        private void IterateC(int n)
        {
            C.Add(2 * X[n - 1] * C[n - 1] + 2 * A[n - 1] * B[n - 1]);
        }

        private void ApproximateSeries()
        {
            for (int n = 1; n < X.Count; n++)
            {
                IterateA(n);
                IterateB(n);
                IterateC(n);
                IterateProbePoints(n);

                BigDecimal error = 0;
                foreach (var P in ProbePoints)
                {
                    error += MagnitudeSquared((A[n] * P[0][0] + B[n] * P[0][1] + C[n] * P[0][2]) - P[n][0]);
                }
                error /= ProbePoints.Length;
                if (error > 1 / Params.Magnification)
                {
                    SkippedIterations = Math.Max(n - 3, 0);
                    return;
                }
            }

            SkippedIterations = X.Count - 1;
            return;
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        public override PixelData Run(T px, T py)
        {
            T x = px;
            T y = py;
            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            Complex zn;

            T deltaReal = Operator.Subtract(x, referenceX);
            T deltaImag = Operator.Subtract(y, referenceY);

            Complex d0 = new Complex(Operator.Convert<T, double>(deltaReal), Operator.Convert<T, double>(deltaImag));

            Complex dn = A[n] * d0 + B[n] * d0 * d0 + C[n] * d0 * d0 * d0;

            // Mandelbrot algorithm
            do
            {

                // dn *= 2*Xn + dn
                dn *= TwoX[n] + dn;

                // dn += d0
                dn += d0;

                // zn = x[iter] * 0.5 + dn
                zn = X[n] + dn;

                n++;

            } while (MagnitudeSquared(zn) < 256 && n < maxIterations);

            if (n > MostIterations)
            {
                newReferenceX = x;
                newReferenceY = y;
                MostIterations = n;
            }

            return new PixelData(zn, n, n >= maxIterations);
        }
    }
}
