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

namespace Mandelbrot.Algorithms
{
    class PerturbationAlgorithmProvider<T> : IAlgorithmProvider<T>
    {
        private IGenericMath<T> TMath;
        private ComplexMath<T> CMath;
        private List<Complex> X, TwoX, A, B, C;

        private CudaDeviceVariable<cuDoubleComplex> dev_points;

        private T Zero;
        private T OneHalf;
        private T One;
        private T Two;
        private T TwoPow8;
        private T NegTwoPow8;
        private T TwoPow10;
        private T NegTwoPow10;

        private T center_real;
        private T center_imag;

        private int MaxIterations;
        private int SkippedIterations;

        private double MagnitudeSquared(Complex a)
        {
            return a.Real * a.Real + a.Imaginary * a.Imaginary;
        }

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        public void Init(IGenericMath<T> TMath, decimal offsetX, decimal offsetY, int maxIterations)
        {
            this.TMath = TMath;
            CMath = new ComplexMath<T>(TMath);
            MaxIterations = maxIterations;

            Zero = TMath.fromInt32(0);
            OneHalf = TMath.fromDouble(0.5);
            One = TMath.fromInt32(1);
            Two = TMath.fromInt32(2);
            TwoPow8 = TMath.fromInt32(256);
            NegTwoPow8 = TMath.fromInt32(-256);
            TwoPow10 = TMath.fromInt32(1024);
            NegTwoPow10 = TMath.fromInt32(-1024);

            center_real = TMath.fromDecimal(offsetX);
            center_imag = TMath.fromDecimal(offsetY);

            A = new List<Complex>();
            B = new List<Complex>();
            C = new List<Complex>();
            X = new List<Complex>();
            TwoX = new List<Complex>();

            GetSurroundingPoints();
            A.Add(new Complex(1, 0));
            B.Add(new Complex(0, 0));
            C.Add(new Complex(0, 0));
            SkippedIterations = ApproximateSeries();
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

                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), center_real);
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), center_imag);
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

        private int ApproximateSeries()
        {
            for (int n = 1; n < X.Count; n++)
            {
                IterateA(n);
                IterateB(n);
                IterateC(n);
                if (MagnitudeSquared(B[n]) < MagnitudeSquared(C[n]))
                {
                    return Math.Max(n - 3, 0);
                }
            }

            return X.Count - 1;
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

            Complex[] d0_tothe = new Complex[4];

            d0_tothe[1] = d0;

            for (int i = 2; i < d0_tothe.Length; i++)
            {
                d0_tothe[i] = d0_tothe[i - 1] * d0_tothe[1];
            }

            Complex dn = A[n] * d0_tothe[1] + B[n] * d0_tothe[2] + C[n] * d0_tothe[3];


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
