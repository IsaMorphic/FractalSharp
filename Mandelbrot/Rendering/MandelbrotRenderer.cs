using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mandelbrot.Utilities;
using Mandelbrot.Rendering.Imaging;

namespace Mandelbrot.Rendering
{
    delegate void FrameStartDelegate();
    delegate void FrameStopDelegate(Bitmap frame);

    delegate void RenderStopDelegate();

    class MandelbrotRenderer
    {
        private DirectBitmap currentFrame;

        private int ThreadCount = Environment.ProcessorCount;
        public long MaxIterations { get; protected set; }
        public double Magnification { get; protected set; }


        private decimal offsetXM;
        private decimal offsetYM;

        private decimal aspectM;

        private int Width;
        private int Height;

        private RGB[] palette;

        private CancellationTokenSource Job;

        public event FrameStartDelegate FrameStart;
        public event FrameStopDelegate FrameEnd;
        public event RenderStopDelegate RenderHalted;

        #region Initialization and Configuration Methods

        public void Initialize(RenderSettings settings, RGB[] newPalette)
        {
            Width = settings.Width;
            Height = settings.Height;

            aspectM = (decimal)Width / (decimal)Height;

            currentFrame = new DirectBitmap(Width, Height);

            palette = newPalette;

        }

        public void Setup(RenderSettings settings)
        {
            Job = new CancellationTokenSource();

            offsetXM = settings.offsetX;
            offsetYM = settings.offsetY;

            Magnification = settings.Magnification;
            MaxIterations = settings.MaxIterations;

            ThreadCount = settings.ThreadCount;
        }

        #endregion

        #region Algorithm Methods
        // Perturbation Theory Algorithm, 
        // produces a list of iteration values used to compute the surrounding points
        private GenericComplex<T>[] get_iteration_list<T, M>(
            M TMath, // Math Object
            T Zero, T TwoPow10, T NegTwoPow10, // Constants
            T center_r, T center_i // Input values
            )
            where M : IGenericMath<T>
        {
            T xn_r = center_r;
            T xn_i = center_i;

            GenericComplex<T>[] iter_list = new GenericComplex<T>[MaxIterations];

            for (int i = 0; i < MaxIterations; i++)
            {
                // pre multiply by two
                T real = TMath.Add(xn_r, xn_r);
                T imag = TMath.Add(xn_i, xn_i);

                T xn_r2 = TMath.Multiply(xn_r, xn_r);
                T xn_i2 = TMath.Multiply(xn_i, xn_i);

                GenericComplex<T> c = new GenericComplex<T>(real, imag);

                iter_list[i] = c;

                // make sure our numbers don't get too big

                // real > 1024 || imag > 1024 || real < -1024 || imag < -1024
                if (TMath.GreaterThan(real, TwoPow10) || TMath.GreaterThan(imag, TwoPow10) ||
                    TMath.LessThan(real, NegTwoPow10) || TMath.LessThan(imag, NegTwoPow10))
                    return iter_list;

                // calculate next iteration, remember real = 2 * xn_r

                // xn_r = xn_r^2 - xn_i^2 + center_r
                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), center_r);
                // xn_i = re * xn_i + center_i
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), center_i);
            }
            return iter_list;
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        private void mandelbrot<T, M>(
            M TMath, ComplexMath<T, M> CMath, // Math Objects
            T Zero, T OneHalf, T Four, // Constants
            T x0, T y0, GenericComplex<T>[] iter_list, // Input values
            out T zn_size, out long iter) // Output values
            where M : IGenericMath<T>
        {
            // Initialize some variables..
            GenericComplex<T> d0 = new GenericComplex<T>(x0, y0);

            GenericComplex<T> dn = d0;

            // Get Max Iterations.  
            long max_iter = MaxIterations - 1;

            // Initialize our iteration count.
            iter = 0;

            zn_size = Zero;

            // Mandelbrot algorithm
            do
            {
                // dn *= iter_list[iter] + dn
                dn = CMath.Multiply(dn, CMath.Add(iter_list[iter], dn));

                // dn += d0
                dn = CMath.Add(dn, d0);

                iter++;

                // zn = x[iter] * 0.5 + dn
                GenericComplex<T> zn = CMath.Add(CMath.Multiply(iter_list[iter], OneHalf), dn);

                zn_size = CMath.MagnitudeSquared(zn);

            } while (TMath.LessThan(zn_size, Four) && iter < max_iter);

        }

        // Smooth Coloring Algorithm
        private Color GetColorFromIterationCount(long iterations, double zn_size)
        {
            double temp_i = iterations;
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(zn_size) / 2;
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            // Rearranging the potential function.
            // Dividing log_zn by log(2) instead of log(N = 1<<8)
            // because we want the entire palette to range from the
            // center to radius 2, NOT our bailout radius.
            temp_i = temp_i + 1 - nu;
            // Grab two colors from the pallete
            RGB color1 = palette[(int)temp_i % (palette.Length - 1)];
            RGB color2 = palette[(int)(temp_i + 1) % (palette.Length - 1)];

            // Lerp between both colors
            RGB final = RGB.LerpColors(color1, color2, temp_i % 1);

            // Return the result.
            return final.toColor();
        }

        #endregion

        #region Rendering Methods

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame<T, M>() where M : IGenericMath<T>, new()
        {
            M TMath = new M();

            ComplexMath<T, M> CMath = new ComplexMath<T, M>(TMath);

            // Fire frame start event
            FrameStart();

            long in_set = 0;

            // Initialize generic values
            T Zero = TMath.fromInt32(0);
            T OneHalf = TMath.fromDouble(0.5);
            T Four = TMath.fromInt32(4);
            T TwoPow10 = TMath.fromInt32(1024);
            T NegTwoPow10 = TMath.fromInt32(-1024);

            // Cast type specific values to the generic type
            T FrameWidth = TMath.fromInt32(Width);
            T FrameHeight = TMath.fromInt32(Height);

            T zoom = TMath.fromDouble(Magnification);

            T offsetX = TMath.fromDecimal(offsetXM);
            T offsetY = TMath.fromDecimal(offsetYM);

            T scaleFactor = TMath.fromDecimal(aspectM);

            // Predefine minimum and maximum values of the plane, 
            // In order to avoid making unnecisary calculations on each pixel.  

            // x_min = -scaleFactor / zoom + offsetX
            // x_max =  scaleFactor / zoom + offsetX
            T x_min = TMath.Divide(TMath.Negate(scaleFactor), zoom);
            T x_max = TMath.Divide(scaleFactor, zoom);

            // y_min = -1 / zoom + offsetY
            // y_max =  1 / zoom + offsetY
            T y_min = TMath.Divide(TMath.fromInt32(-1), zoom);
            T y_max = TMath.Divide(TMath.fromInt32(1), zoom);

            var iter_list = get_iteration_list<T, M>(TMath, Zero, TwoPow10, NegTwoPow10, offsetX, offsetY);

            var loop = Parallel.For(0, Width, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                T x0 = Utils.Map<T, M>(TMath.fromInt32(px), Zero, FrameWidth, x_min, x_max);

                for (int py = 0; py < Height; py++)
                {
                    T y0 = Utils.Map<T, M>(TMath.fromInt32(py), Zero, FrameHeight, y_min, y_max);

                    T zn_size = Zero;

                    // Initialize our iteration count.
                    long iteration = 0;

                    mandelbrot<T, M>(TMath, CMath, Zero, OneHalf, Four, x0, y0, iter_list, out zn_size, out iteration);

                    // If x squared plus y squared is outside the set, give it a fancy color.
                    if (TMath.GreaterThan(zn_size, Four)) // xx + yy > 4
                    {
                        Color PixelColor = GetColorFromIterationCount(iteration, TMath.toDouble(zn_size));
                        currentFrame.SetPixel(px, py, PixelColor);
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        currentFrame.SetPixel(px, py, Color.Black);
                        Interlocked.Increment(ref in_set);
                    }
                }
            });

            if (in_set == Width * Height) StopRender();

            Bitmap newFrame = (Bitmap)currentFrame.Bitmap.Clone();
            FrameEnd(newFrame);
        }

        // Method that signals the render process to stop.  
        public void StopRender()
        {
            Job.Cancel();
            RenderHalted();
        }

        #endregion

    }
}
