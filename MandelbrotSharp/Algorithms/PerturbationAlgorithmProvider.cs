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
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public class PerturbationAlgorithmProvider<T> : AlgorithmProvider<T> where T : struct
    {
        Random Random;
        private List<Complex> X, TwoX, A, B, C;
        private List<Complex[]>[] ProbePoints;
        private Complex<T> newReferencePoint;
        private Complex<T> referencePoint;

        private int SkippedIterations;
        private int MostIterations;
        private int PreviousMaxIterations;

        [Parameter(DefaultValue = true)]
        public bool ShouldUseSeriesApproximation;

        [Parameter(DefaultValue = 20)]
        public int NumProbePoints;

        public PerturbationAlgorithmProvider()
        {
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
            Random = new Random();
            ProbePoints = new List<Complex[]>[NumProbePoints];

            A.Clear();
            B.Clear();
            C.Clear();
            X.Clear();
            TwoX.Clear();

            referencePoint = newReferencePoint;
        }

        protected override void Initialize(CancellationToken token)
        {
            for (int i = 0; i < ProbePoints.Length; i++)
            {
                ProbePoints[i] = new List<Complex[]>();
                BigDecimal real = (Random.NextDouble() * 4 - 2) / Params.Magnification;
                BigDecimal imag = (Random.NextDouble() * 4 - 2) / Params.Magnification;
                var point = (Complex)(new Complex<BigDecimal>(real, imag) + Params.Location);
                ProbePoints[i].Add(new Complex[3] { point, point * point, point * point * point });
            }

            A.Add(new Complex(1, 0));
            B.Add(new Complex(0, 0));
            C.Add(new Complex(0, 0));

            IterateReferencePoint(token);

            MostIterations = X.Count - 1;

            if (ShouldUseSeriesApproximation)
                ApproximateSeries();

            PreviousMaxIterations = Params.MaxIterations;
            base.OnParamsUpdated();
        }

        private double MagnitudeSquared(Complex a)
        {
            return a.Real * a.Real + a.Imaginary * a.Imaginary;
        }

        public void IterateReferencePoint(CancellationToken token)
        {
            Complex<T> x0, xn = x0 = referencePoint;

            for (int i = 0; i < Params.MaxIterations; i++)
            {
                token.ThrowIfCancellationRequested();

                X.Add((Complex)xn);
                TwoX.Add((Complex)(xn * 2));

                if (xn.MagnitudeSqu > 4)
                    break;

                xn = xn * xn + x0;
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
        protected override PointData Run(Complex<T> point)
        {
            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            Complex zn;

            Complex d0 = (Complex)(point - referencePoint);
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

            var pointData = new PointData(zn, n, n < maxIterations);

            // Only test a small percentage of points
            if (Random.NextDouble() > .05)
                return pointData;

            // Iterate this small percentage a little further to find good references.  
            Complex<T> y = (Complex<T>)zn;
            while (y.MagnitudeSqu < 256 && n < Math.Min(MostIterations + 1, maxIterations * 1.2))
            {
                y = y * y + point;
                n++;
            }

            // May the best point win.
            if (n > MostIterations)
            {
                newReferencePoint = point;
                MostIterations = n;
            }

            return pointData;
        }
    }
}
