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
using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MandelbrotSharp.Algorithms
{
    public class SquareMandelbrotPParams<TNumber> : AlgorithmParams<TNumber>
        where TNumber : struct
    {
        public int NumProbePoints { get; set; }
        public bool ShouldUseSeriesApproximation { get; set; }

        public Complex<TNumber> Reference { get; set; }

        public override IAlgorithmParams Copy()
        {
            return new SquareMandelbrotPParams<TNumber>
            {
                MaxIterations = MaxIterations,
                Magnification = Magnification,
                Location = Location,

                NumProbePoints = NumProbePoints,
                ShouldUseSeriesApproximation = ShouldUseSeriesApproximation,
                Reference = Reference
            };
        }
    }

    public class SquareMandelbrotPAlgorithm<TNumber> 
        : AlgorithmProvider<TNumber, SquareMandelbrotPParams<TNumber>>, 
          IAlgorithmProvider<TNumber>
        where TNumber : struct
    {
        private static readonly Number<TNumber> One = Number<TNumber>.From(1);
        private static readonly Number<TNumber> Two = Number<TNumber>.From(2);

        private static readonly Number<double> dOne = Number<double>.From(1);
        private static readonly Number<double> dTwo = Number<double>.From(2);

        private Random Random;

        private List<Complex<double>> X, TwoX, A, B, C;
        private List<Complex<double>[]>[] ProbePoints;

        private int SkippedIterations;

        public override Rectangle<TNumber> GetOutputBounds(Number<TNumber> aspectRatio)
        {
            Number<TNumber> xScale = aspectRatio * Two / Params.Magnification;

            Number<TNumber> xMin = -xScale + Params.Location.Real - Params.Reference.Real;
            Number<TNumber> xMax = xScale + Params.Location.Real - Params.Reference.Real;

            Number<TNumber> yScale = Two / Params.Magnification;

            Number<TNumber> yMin = yScale + Params.Location.Imag - Params.Reference.Imag;
            Number<TNumber> yMax = -yScale + Params.Location.Imag - Params.Reference.Imag;

            return new Rectangle<TNumber>(xMin, xMax, yMin, yMax);
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        public override PointData Run(Complex<TNumber> point)
        {
            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            Complex<double> zn;
            Complex<double> d0 = point.As<double>();
            Complex<double> dn = A[n] * d0 + B[n] * d0 * d0 + C[n] * d0 * d0 * d0;

            // Mandelbrot algorithm
            do
            {

                dn *= TwoX[n] + dn;
                dn += d0;

                zn = X[n] + dn;
                n++;

            } while (zn.MagnitudeSqu < 256 && n < maxIterations);

            return new PointData(zn, n, n < maxIterations);
        }

        protected override void Initialize(CancellationToken token)
        {
            // Initialize Lists
            A = new List<Complex<double>>();
            B = new List<Complex<double>>();
            C = new List<Complex<double>>();
            X = new List<Complex<double>>();
            TwoX = new List<Complex<double>>();

            Random = new Random();
            ProbePoints = new List<Complex<double>[]>[Params.NumProbePoints];

            for (int i = 0; i < ProbePoints.Length; i++)
            {
                ProbePoints[i] = new List<Complex<double>[]>();

                Number<TNumber> real = Number<TNumber>.From(Random.NextDouble() * 4 - 2) / Params.Magnification;
                Number<TNumber> imag = Number<TNumber>.From(Random.NextDouble() * 4 - 2) / Params.Magnification;

                Complex<TNumber> offset = new Complex<TNumber>(real, imag);
                Complex<double> point = (offset + Params.Location).As<double>();

                ProbePoints[i].Add(new Complex<double>[3] { point, point * point, point * point * point });
            }

            A.Add(new Complex<double>(1.0, 0.0));
            B.Add(new Complex<double>(0.0, 0.0));
            C.Add(new Complex<double>(0.0, 0.0));

            IterateReferencePoint(token);

            if (Params.ShouldUseSeriesApproximation)
                ApproximateSeries();
        }

        private void IterateReferencePoint(CancellationToken token)
        {
            Complex<TNumber> x0, xn = x0 = Params.Reference;

            for (int i = 0; i < Params.MaxIterations; i++)
            {
                token.ThrowIfCancellationRequested();

                Complex<double> smallXn = xn.As<double>();

                X.Add(smallXn);
                TwoX.Add(smallXn + smallXn);

                if (smallXn.MagnitudeSqu > 256)
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
                P.Add(new Complex<double>[] { dn });
            }
        }

        private void IterateA(int n)
        {
            A.Add(dTwo * X[n - 1] * A[n - 1] + dOne);
        }

        private void IterateB(int n)
        {
            B.Add(dTwo * X[n - 1] * B[n - 1] + A[n - 1] * A[n - 1]);
        }

        private void IterateC(int n)
        {
            C.Add(dTwo * X[n - 1] * C[n - 1] + dTwo * A[n - 1] * B[n - 1]);
        }

        private void ApproximateSeries()
        {
            for (int n = 1; n < X.Count; n++)
            {
                IterateA(n);
                IterateB(n);
                IterateC(n);
                IterateProbePoints(n);

                Number<TNumber> error = Number<TNumber>.Zero;
                foreach (var P in ProbePoints)
                {
                    Complex<double> approximation = A[n] * P[0][0] + B[n] * P[0][1] + C[n] * P[0][2];
                    error += (approximation - P[n][0]).MagnitudeSqu.As<TNumber>();
                }
                error /= Number<TNumber>.From(ProbePoints.Length);
                if (error > One / Params.Magnification)
                {
                    SkippedIterations = Math.Max(n - 3, 0);
                    return;
                }
            }

            SkippedIterations = X.Count - 1;
            return;
        }
    }
}
