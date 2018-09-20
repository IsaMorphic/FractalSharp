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
    class PerturbationAlgorithmProvider<T> : IAlgorithmProvider<T>
    {
        private IGenericMath<T> TMath;
        private List<GenericComplex<T>> pointsList;

        private CudaKernel renderKernel;
        private CudaKernel pointsKernel;

        private T Zero;
        private T OneHalf;
        private T TwoPow8;
        private T NegTwoPow8;
        private T TwoPow10;
        private T NegTwoPow10;

        private T center_real;
        private T center_imag;

        private int MaxIterations;

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        public void Init(IGenericMath<T> TMath, T offsetX, T offsetY, int maxIterations)
        {
            this.TMath = TMath;
            MaxIterations = maxIterations;

            Zero = TMath.fromInt32(0);
            OneHalf = TMath.fromDouble(0.5);
            TwoPow8 = TMath.fromInt32(256);
            NegTwoPow8 = TMath.fromInt32(-256);
            TwoPow10 = TMath.fromInt32(1024);
            NegTwoPow10 = TMath.fromInt32(-1024);

            center_real = offsetX;
            center_imag = offsetY;

            pointsList = GetSurroundingPoints();
        }

        public List<GenericComplex<T>> GetSurroundingPoints()
        {
            T xn_r = center_real;
            T xn_i = center_imag;

            var x = new List<GenericComplex<T>>();

            for (int i = 0; i < MaxIterations; i++)
            {
                // pre multiply by two
                T real = TMath.Add(xn_r, xn_r);
                T imag = TMath.Add(xn_i, xn_i);

                T xn_r2 = TMath.Multiply(xn_r, xn_r);
                T xn_i2 = TMath.Multiply(xn_i, xn_i);

                GenericComplex<T> c = new GenericComplex<T>(real, imag);

                x.Add(c);

                // make sure our numbers don't get too big

                // real > 1024 || imag > 1024 || real < -1024 || imag < -1024
                if (TMath.GreaterThan(real, TwoPow10) || TMath.GreaterThan(imag, TwoPow10) ||
                    TMath.LessThan(real, NegTwoPow10) || TMath.LessThan(imag, NegTwoPow10))
                    break;

                // calculate next iteration, remember real = 2 * xn_r

                // xn_r = xn_r^2 - xn_i^2 + center_r
                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), center_real);
                // xn_i = re * xn_i + center_i
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), center_imag);
            }
            return x;
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        public PixelData<T> Run(T x0, T y0)
        {
            ComplexMath<T> CMath = new ComplexMath<T>(TMath);

            // Get max iterations.  
            int maxIterations = pointsList.Count - 1;

            // Initialize our iteration count.
            int iterCount = 0;

            // Initialize some variables...
            GenericComplex<T> zn;

            GenericComplex<T> d0 = new GenericComplex<T>(x0, y0);

            GenericComplex<T> dn = d0;

            T znMagn = Zero;

            // Mandelbrot algorithm
            do
            {

                // dn *= iter_list[iter] + dn
                dn = CMath.Multiply(dn, CMath.Add(pointsList[iterCount], dn));

                // dn += d0
                dn = CMath.Add(dn, d0);

                iterCount++;

                // zn = x[iter] * 0.5 + dn
                zn = CMath.Add(CMath.Multiply(pointsList[iterCount], OneHalf), dn);

                znMagn = CMath.MagnitudeSquared(zn);

            } while (TMath.LessThan(znMagn, TwoPow8) && iterCount < maxIterations);

            return new PixelData<T>(znMagn, iterCount, iterCount < maxIterations);
        }

        public void GPUInit(CudaContext ctx)
        {
            renderKernel = ctx.LoadKernelPTX(Resources.Kernel, "perturbation");
            pointsKernel = ctx.LoadKernelPTX(Resources.Kernel, "get_points");

            pointsKernel.BlockDimensions = 1;
            pointsKernel.GridDimensions = 1;
        }

        public int[] GPUFrame(int[] palette, int width, int height, double xMax, double yMax, double offsetX, double offsetY, int maxIter)
        {
            renderKernel.BlockDimensions = new dim3(16, 9);
            renderKernel.GridDimensions = new dim3(width / 16, height / 9);

            var dev_points = new CudaDeviceVariable<cuDoubleComplex>(maxIter);
            CudaDeviceVariable<int> dev_pointCount = 0;

            pointsKernel.Run(dev_points.DevicePointer, dev_pointCount.DevicePointer, offsetX, offsetY, maxIter);

            int pointCount = dev_pointCount;

            var dev_image = new CudaDeviceVariable<int>(width * height);
            CudaDeviceVariable<int> dev_palette = palette;

            renderKernel.Run(dev_image.DevicePointer, dev_palette.DevicePointer, palette.Length, dev_points.DevicePointer, pointCount, width, height, xMax, yMax);

            int[] raw_image = dev_image;

            dev_points.Dispose();
            dev_pointCount.Dispose();
            dev_image.Dispose();
            dev_palette.Dispose();

            return raw_image;
        }
    }
}
