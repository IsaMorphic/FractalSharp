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

        private CudaKernel gpuKernel;
        private CudaDeviceVariable<cuDoubleComplex> dev_points;

        private T Zero;
        private T OneHalf;
        private T TwoPow8;
        private T NegTwoPow8;
        private T TwoPow10;
        private T NegTwoPow10;

        private decimal center_real;
        private decimal center_imag;

        private int MaxIterations;

        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        public void Init(IGenericMath<T> TMath, decimal offsetX, decimal offsetY, int maxIterations)
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
            decimal xn_r = center_real;
            decimal xn_i = center_imag;

            var x = new List<GenericComplex<T>>();

            for (int i = 0; i < MaxIterations; i++)
            {
                // pre multiply by two
                decimal real = xn_r + xn_r;
                decimal imag = xn_i + xn_i;

                decimal xn_r2 = xn_r * xn_r;
                decimal xn_i2 = xn_i * xn_i;

                GenericComplex<T> c = new GenericComplex<T>(TMath.fromDecimal(real), TMath.fromDecimal(imag));

                x.Add(c);

                // make sure our numbers don't get too big

                if (real >  1024 || imag >  1024 ||
                    real < -1024 || imag < -1024)
                    break;

                // calculate next iteration, remember real = 2 * xn_r

                xn_r = xn_r2 - xn_i2 + center_real;
                xn_i = real * xn_i + center_imag;
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

        public void GPUInit(CudaContext ctx, byte[] ptxImage, dim3 gridDim, dim3 blockDim)
        {
            gpuKernel = ctx.LoadKernelPTX(Resources.Kernel, "perturbation");

            gpuKernel.GridDimensions = gridDim;
            gpuKernel.BlockDimensions = blockDim;
        }

        public void GPUPreFrame()
        {
            cuDoubleComplex[] cuDoubles =
                new cuDoubleComplex[pointsList.Count];
            for (var i = 0; i < cuDoubles.Length; i++)
            {
                GenericComplex<T> complex = pointsList[i];
                cuDoubles[i] = new cuDoubleComplex(
                        TMath.toDouble(complex.real),
                        TMath.toDouble(complex.imag));
            }

            dev_points = cuDoubles;
        }

        public void GPUPostFrame()
        {
            dev_points.Dispose();
        }

        public void GPUCell(
            CudaDeviceVariable<int> dev_image,
            CudaDeviceVariable<int> dev_palette,
            int cell_x, int cell_y,
            int cellWidth, int cellHeight,
            int totalCells_x, int totalCells_y,
            double xMax, double yMax, 
            int chunkSize)
        {
            gpuKernel.Run(
                dev_image.DevicePointer,
                dev_palette.DevicePointer, dev_palette.Size,
                dev_points.DevicePointer, dev_points.Size,
                cell_x, cell_y,
                cellWidth, cellHeight,
                totalCells_x, totalCells_y,
                xMax, yMax);
        }
    }
}
