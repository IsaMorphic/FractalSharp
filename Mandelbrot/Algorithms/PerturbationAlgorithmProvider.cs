using ManagedCuda;
using ManagedCuda.VectorTypes;
using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using Mandelbrot.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Mandelbrot.Rendering;

namespace Mandelbrot.Algorithms
{
    class PerturbationAlgorithmProvider<T> : IAlgorithmProvider<T>
    {
        private IGenericMath<T> TMath;
        private ComplexMath<T> CMath;
        private List<Complex> X, TwoX, A, B, C;
        private List<Complex[]>[] ProbePoints = new List<Complex[]>[20];

        //private CudaDeviceVariable<cuDoubleComplex> dev_points;

        private T Zero;
        private T TwoPow8;
        private T TwoPow10;

        private T center_real;
        private T center_imag;

        private int MaxIterations;
        private int SkippedIterations;

        private double Magnification;

        private double MagnitudeSquared(Complex a)
        {
            return a.Real * a.Real + a.Imaginary * a.Imaginary;
        }

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        public void Init(IGenericMath<T> TMath, RenderSettings settings)
        {
            this.TMath = TMath;
            CMath = new ComplexMath<T>(TMath);
            MaxIterations = settings.MaxIterations;
            Magnification = settings.Magnification;

            Zero = TMath.fromInt32(0);
            TwoPow8 = TMath.fromInt32(256);
            TwoPow10 = TMath.fromInt32(1024);

            center_real = TMath.fromDecimal(settings.offsetX);
            center_imag = TMath.fromDecimal(settings.offsetY);

            A = new List<Complex>();
            B = new List<Complex>();
            C = new List<Complex>();
            X = new List<Complex>();
            TwoX = new List<Complex>();

            Random random = new Random();
            for (int i = 0; i < ProbePoints.Length; i++)
            {
                ProbePoints[i] = new List<Complex[]>();
                var point = new Complex((random.NextDouble() * 4 - 2) / Magnification, (random.NextDouble() * 4 - 2) / Magnification);
                ProbePoints[i].Add(new Complex[3] { point, point * point, point * point * point });
            }

            GetSurroundingPoints();
            A.Add(new Complex(1, 0));
            B.Add(new Complex(0, 0));
            C.Add(new Complex(0, 0));
            ApproximateSeries();
        }

        public void GetSurroundingPoints()
        {
            T xn_r = center_real;
            T xn_i = center_imag;

            for (int i = 0; i < MaxIterations; i++)
            {
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
                if (MagnitudeSquared(X[i]) > 4)
                    break;


                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), center_real);
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), center_imag);
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

                double error = 0;
                foreach (var P in ProbePoints)
                {
                    error += MagnitudeSquared((A[n] * P[0][0] + B[n] * P[0][1] + C[n] * P[0][2]) - P[n][0]);
                }
                error /= ProbePoints.Length;
                if (error > 1 / Magnification)
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
        public PixelData<T> Run(T x0, T y0)
        {

            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            Complex zn;

            Complex d0 = new Complex(TMath.toDouble(x0), TMath.toDouble(y0));

            Complex dn = A[n] * d0 + B[n] * d0 * d0 + C[n] * d0 * d0 * d0;


            T znMagn = Zero;

            // Mandelbrot algorithm
            do
            {

                // dn *= 2*Xn + dn
                dn *= TwoX[n] + dn;

                // dn += d0
                dn += d0;

                n++;

                // zn = x[iter] * 0.5 + dn
                zn = X[n] + dn;

                znMagn = TMath.fromDouble(MagnitudeSquared(zn));

            } while (TMath.LessThan(znMagn, TwoPow8) && n < maxIterations);

            return new PixelData<T>(znMagn, n, n < maxIterations);
        }

        //    public override void GPUInit(CudaContext ctx, byte[] ptxImage, dim3 gridDim, dim3 blockDim)
        //    {
        //        gpuKernel = ctx.LoadKernelPTX(Resources.Kernel, "perturbation");

        //        gpuKernel.GridDimensions = gridDim;
        //        gpuKernel.BlockDimensions = blockDim;
        //    }

        //    public override void GPUPreFrame()
        //    {
        //        cuDoubleComplex[] cuDoubles =
        //            new cuDoubleComplex[X.Count];
        //        for (var i = 0; i < cuDoubles.Length; i++)
        //        {
        //            GenericComplex<T> complex = X[i];
        //            cuDoubles[i] = new cuDoubleComplex(
        //                    TMath.toDouble(complex.real),
        //                    TMath.toDouble(complex.imag));
        //        }

        //        dev_points = cuDoubles;
        //    }

        //    public override void GPUPostFrame()
        //    {
        //        dev_points.Dispose();
        //    }

        //    public override void GPUCell(
        //        CudaDeviceVariable<int> dev_image,
        //        CudaDeviceVariable<int> dev_palette,
        //        int cell_x, int cell_y,
        //        int cellWidth, int cellHeight,
        //        int totalCells_x, int totalCells_y,
        //        double xMax, double yMax,
        //        int chunkSize, int maxChunkSize)
        //    {
        //        gpuKernel.Run(
        //            dev_image.DevicePointer,
        //            dev_palette.DevicePointer, dev_palette.Size,
        //            dev_points.DevicePointer, dev_points.Size,
        //            cell_x, cell_y,
        //            cellWidth, cellHeight,
        //            totalCells_x, totalCells_y,
        //            xMax, yMax,
        //            chunkSize, maxChunkSize);
        //    }
    }
}
