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

namespace Mandelbrot.Algorithms
{
    class PerturbationAlgorithmProvider<T> : GPUAlgorithmProvider<T>
    {
        private IGenericMath<T> TMath;
        private ComplexMath<T> CMath;
        private List<GenericComplex<T>> X, TwoX, A, B, C;

        private CudaDeviceVariable<cuDoubleComplex> dev_points;

        private T Zero;
        private T OneHalf;
        private T One;
        private T Two;
        private T TwoPow8;
        private T NegTwoPow8;
        private T TwoPow10;
        private T NegTwoPow10;

        private decimal center_real;
        private decimal center_imag;

        private int MaxIterations;
        private int SkippedIterations;

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        public override void Init(IGenericMath<T> TMath, decimal offsetX, decimal offsetY, int maxIterations)
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

            center_real = offsetX;
            center_imag = offsetY;

            A = new List<GenericComplex<T>>();
            B = new List<GenericComplex<T>>();
            C = new List<GenericComplex<T>>();
            X = new List<GenericComplex<T>>();
            TwoX = new List<GenericComplex<T>>();

            GetSurroundingPoints();
            A.Add(new GenericComplex<T>(One, Zero));
            B.Add(new GenericComplex<T>(Zero, Zero));
            C.Add(new GenericComplex<T>(Zero, Zero));
            SkippedIterations = ApproximateSeries();
        }

        public void GetSurroundingPoints()
        {
            decimal xn_r = center_real;
            decimal xn_i = center_imag;

            for (int i = 0; i < MaxIterations; i++)
            {
                // pre multiply by two
                decimal real = xn_r + xn_r;
                decimal imag = xn_i + xn_i;

                decimal xn_r2 = xn_r * xn_r;
                decimal xn_i2 = xn_i * xn_i;

                GenericComplex<T> c = new GenericComplex<T>(TMath.fromDecimal(xn_r), TMath.fromDecimal(xn_i));
                GenericComplex<T> two_c = new GenericComplex<T>(TMath.fromDecimal(real), TMath.fromDecimal(imag));

                X.Add(c);
                TwoX.Add(two_c);

                // calculate next iteration, remember real = 2 * xn_r

                xn_r = xn_r2 - xn_i2 + center_real;
                xn_i = real * xn_i + center_imag;
            }
        }

        private void IterateA(int n)
        {
            A.Add(
                CMath.Add(
                    CMath.Multiply(
                        CMath.Multiply(X[n - 1], A[n - 1]),
                    Two),
                new GenericComplex<T>(One, Zero)));
        }

        private void IterateB(int n)
        {
            B.Add(
                CMath.Add(
                    CMath.Multiply(
                        CMath.Multiply(X[n - 1], B[n - 1]),
                    Two),
                CMath.Multiply(A[n - 1], A[n - 1])));
        }

        private void IterateC(int n)
        {
            C.Add(
                CMath.Multiply(
                    CMath.Add(
                        CMath.Multiply(X[n - 1], C[n - 1]),
                        CMath.Multiply(A[n - 1], B[n - 1])),
                Two));
        }

        private int ApproximateSeries()
        {
            for (int n = 1; n < X.Count; n++)
            {
                IterateA(n);
                IterateB(n);
                IterateC(n);
                if (TMath.LessThan(CMath.MagnitudeSquared(B[n]), CMath.MagnitudeSquared(C[n])))
                {
                    return Math.Max(n - 3, 0);
                }
            }

            return X.Count - 1;
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        public override PixelData<T> Run(T x0, T y0)
        {

            // Get max iterations.  
            int maxIterations = X.Count - 1;

            // Initialize our iteration count.
            int n = SkippedIterations;

            // Initialize some variables...
            GenericComplex<T> zn;

            GenericComplex<T> d0 = new GenericComplex<T>(x0, y0);

            GenericComplex<T>[] d0_tothe = new GenericComplex<T>[4];

            d0_tothe[1] = d0;

            for (int i = 2; i < d0_tothe.Length; i++)
            {
                d0_tothe[i] = CMath.Multiply(d0_tothe[i - 1], d0_tothe[1]);
            }

            GenericComplex<T> dn = CMath.Add(
                CMath.Multiply(A[n], d0_tothe[1]),
                CMath.Add(CMath.Multiply(B[n], d0_tothe[2]),
                CMath.Multiply(C[n], d0_tothe[3])));


            T znMagn = Zero;

            // Mandelbrot algorithm
            do
            {

                // dn *= 2*Xn + dn
                dn = CMath.Multiply(dn, CMath.Add(TwoX[n], dn));

                // dn += d0
                dn = CMath.Add(dn, d0);

                n++;

                // zn = x[iter] * 0.5 + dn
                zn = CMath.Add(X[n], dn);

                znMagn = CMath.MagnitudeSquared(zn);

            } while (TMath.LessThan(znMagn, TwoPow8) && n < maxIterations);

            return new PixelData<T>(znMagn, n, n < maxIterations);
        }

        public override void GPUInit(CudaContext ctx, byte[] ptxImage, dim3 gridDim, dim3 blockDim)
        {
            gpuKernel = ctx.LoadKernelPTX(Resources.Kernel, "perturbation");

            gpuKernel.GridDimensions = gridDim;
            gpuKernel.BlockDimensions = blockDim;
        }

        public override void GPUPreFrame()
        {
            cuDoubleComplex[] cuDoubles =
                new cuDoubleComplex[X.Count];
            for (var i = 0; i < cuDoubles.Length; i++)
            {
                GenericComplex<T> complex = X[i];
                cuDoubles[i] = new cuDoubleComplex(
                        TMath.toDouble(complex.real),
                        TMath.toDouble(complex.imag));
            }

            dev_points = cuDoubles;
        }

        public override void GPUPostFrame()
        {
            dev_points.Dispose();
        }

        public override void GPUCell(
            CudaDeviceVariable<int> dev_image,
            CudaDeviceVariable<int> dev_palette,
            int cell_x, int cell_y,
            int cellWidth, int cellHeight,
            int totalCells_x, int totalCells_y,
            double xMax, double yMax,
            int chunkSize, int maxChunkSize)
        {
            gpuKernel.Run(
                dev_image.DevicePointer,
                dev_palette.DevicePointer, dev_palette.Size,
                dev_points.DevicePointer, dev_points.Size,
                cell_x, cell_y,
                cellWidth, cellHeight,
                totalCells_x, totalCells_y,
                xMax, yMax,
                chunkSize, maxChunkSize);
        }
    }
}
