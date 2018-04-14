using Hybridizer.Runtime.CUDAImports;
using System.Drawing;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
namespace MandelBrot
{
    class GPUFractal
    {
        [Kernel]
        public static double IterCount(double cx, double cy, int maxiter)
        {
            double result = 0;
            int iteration = 0;
            double x = 0.0, y = 0.0, xx = 0.0, yy = 0.0;
            while (xx + yy <= 4.0 && iteration < maxiter)
            {
                xx = x * x;
                yy = y * y;
                double xtmp = xx - yy + cx;
                y = 2.0 * x * y + cy;
                x = xtmp;
                iteration++;
            }
            if (xx + yy > 4.0)
            {
                double temp_i = iteration;
                // sqrt of inner term removed using log simplification rules.
                double log_zn = Math.Log((double)xx + (double)yy) / 2;
                double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
                // Rearranging the potential function.
                // Dividing log_zn by log(2) instead of log(N = 1<<8)
                // because we want the entire palette to range from the
                // center to radius 2, NOT our bailout radius.
                temp_i = temp_i + 1 - nu;

                result = temp_i;
            }
            return result;
        }

        [EntryPoint("run")]
        public static void Run(double[] data_in, int lineFrom, int lineTo, int N, int M, int frameNum, int maxiter)
        {
            double offsetX = -0.743643887037158704752191506114774;
            double offsetY = 0.131825904205311970493132056385139;
            double fromX = (double)N / (double)M;
            double fromY = -1;
            double w = fromX * 2 / N;
            double h = -fromY * 2 / M;
            double zoom = Math.Pow(frameNum, frameNum / 100.0);
            for (int line = lineFrom + threadIdx.y + blockDim.y * blockIdx.y; line < lineTo; line += gridDim.y * blockDim.y)
            {
                for (int j = threadIdx.x + blockIdx.x * blockDim.x; j < N; j += blockDim.x * gridDim.x)
                {
                    double x = j * (w / zoom) - (fromX / zoom) + offsetX;
                    double y = (fromY / zoom) + line * (h / zoom) + offsetY;
                    data_in[line * N + j] = IterCount(x, y, maxiter);
                }
            }
        }

        private static dynamic wrapper;

        public static void ComputeImage(double[] data, int N, int M, int frameNum, int maxIter, RGB[] palette)
        {
            wrapper.Run(data, 0, M, N, M, frameNum, maxIter);
        }

        public static void RenderFrame(ref DirectBitmap Frame, int frameNum, int maxiter, RGB[] palette)
        {
            int N = Frame.Width, M = Frame.Height;
            // Init data array
            double[] frame_data = new double[N * M];

            HybRunner runner = HybRunner.Cuda("MandelBrot_CUDA.vs2017.dll").SetDistrib(32, 32, 16, 16, 1, 0);
            wrapper = runner.Wrap(new GPUFractal());

            ComputeImage(frame_data, N, M, frameNum, maxiter, palette);

            for (int x = 0; x < N; ++x)
            {
                for (int y = 0; y < M; ++y)
                {
                    double temp_i = frame_data[y * N + x];

                    RGB color1 = palette[(int)temp_i % palette.Length];
                    RGB color2 = palette[(int)(temp_i + 1) % palette.Length];

                    // Linear interpolate red, green, and blue values.
                    int final_red = (int)Utils.lerp(color1.red, color2.red, temp_i % 1);

                    int final_green = (int)Utils.lerp(color1.green, color2.green, temp_i % 1);

                    int final_blue = (int)Utils.lerp(color1.blue, color2.blue, temp_i % 1);

                    // Construct a final color with the interpolated values.
                    RGB finalColor = new RGB(final_red, final_green, final_blue);

                    // Then set our pixel to that color.  
                    Frame.SetPixel(x, y, Color.FromArgb(finalColor.red, finalColor.green, finalColor.blue));
                }
            }
        }
    }
}
