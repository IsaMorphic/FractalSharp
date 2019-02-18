using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public class PerturbationAlgorithmProvider<T> : AlgorithmProvider<T>
    {
        private List<Complex> X, TwoX, A, B, C;
        private List<Complex[]>[] ProbePoints = new List<Complex[]>[20];
        private T referenceX, referenceY;

        private T Zero, Four;

        private int SkippedIterations;

        public PerturbationAlgorithmProvider(GenericMath<T> TMath) : base(TMath)
        {
            Zero = TMath.fromInt32(0);
            Four = TMath.fromInt32(4);

            A = new List<Complex>();
            B = new List<Complex>();
            C = new List<Complex>();
            X = new List<Complex>();
            TwoX = new List<Complex>();
        }

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points

        protected override void ParamsUpdated()
        {
            A.Clear();
            B.Clear();
            C.Clear();
            X.Clear();
            TwoX.Clear();

            referenceX = TMath.fromBigDecimal(Params.offsetX);
            referenceY = TMath.fromBigDecimal(Params.offsetY);

            Random random = new Random();
            for (int i = 0; i < ProbePoints.Length; i++)
            {
                ProbePoints[i] = new List<Complex[]>();
                var point = new Complex((double)((random.NextDouble() * 4 - 2) / Params.Magnification), (double)((random.NextDouble() * 4 - 2) / Params.Magnification));
                ProbePoints[i].Add(new Complex[3] { point, point * point, point * point * point });
            }

            A.Add(new Complex(1, 0));
            B.Add(new Complex(0, 0));
            C.Add(new Complex(0, 0));

            IterateReferencePoint();
            ApproximateSeries();
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
                T real = TMath.Add(xn_r, xn_r);
                T imag = TMath.Add(xn_i, xn_i);

                T xn_r2 = TMath.Multiply(xn_r, xn_r);
                T xn_i2 = TMath.Multiply(xn_i, xn_i);

                Complex c = new Complex(TMath.toDouble(xn_r), TMath.toDouble(xn_i));
                Complex two_c = new Complex(TMath.toDouble(real), TMath.toDouble(imag));

                X.Add(c);
                TwoX.Add(two_c);
                // calculate next iteration, remember real = 2 * xn_r
                if (TMath.GreaterThan(TMath.Add(xn_r2, xn_i2), Four))
                    break;


                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), x0_r);
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), x0_i);
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
            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            Complex zn;

            Complex d0 = new Complex(TMath.toDouble(TMath.Subtract(px, referenceX)), TMath.toDouble(TMath.Subtract(py, referenceY)));

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
            return new PixelData(MagnitudeSquared(zn), n, n >= maxIterations);
        }
    }
}
