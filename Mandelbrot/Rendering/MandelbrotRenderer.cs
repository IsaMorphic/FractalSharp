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
        private DirectBitmap CurrentFrame;

        private int    ThreadCount = Environment.ProcessorCount;
        public  int    MaxIterations { get; protected set; }
        public  double Magnification { get; protected set; }


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

            CurrentFrame = new DirectBitmap(Width, Height);

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
        private List<GenericComplex<T>> GetIterationList<T, M>(
            T Zero, T TwoPow10, T NegTwoPow10, // Constants
            T center_real, T center_imag // Input values
            ) where M : IGenericMath<T>, new()
        {
            M TMath = new M();

            T xn_r = center_real;
            T xn_i = center_imag;

            List<GenericComplex<T>> iterList = new List<GenericComplex<T>>();

            for (int i = 0; i < MaxIterations; i++)
            {
                // pre multiply by two
                T real = TMath.Add(xn_r, xn_r);
                T imag = TMath.Add(xn_i, xn_i);

                T xn_r2 = TMath.Multiply(xn_r, xn_r);
                T xn_i2 = TMath.Multiply(xn_i, xn_i);

                GenericComplex<T> c = new GenericComplex<T>(real, imag);

                iterList.Add(c);

                // make sure our numbers don't get too big

                // real > 1024 || imag > 1024 || real < -1024 || imag < -1024
                if (TMath.GreaterThan(real, TwoPow10) || TMath.GreaterThan(imag, TwoPow10) ||
                    TMath.LessThan(real, NegTwoPow10) || TMath.LessThan(imag, NegTwoPow10))
                    return iterList;

                // calculate next iteration, remember real = 2 * xn_r

                // xn_r = xn_r^2 - xn_i^2 + center_r
                xn_r = TMath.Add(TMath.Subtract(xn_r2, xn_i2), center_real);
                // xn_i = re * xn_i + center_i
                xn_i = TMath.Add(TMath.Multiply(real, xn_i), center_imag);
            }
            return iterList;
        }

        // Non-Traditional Mandelbrot algorithm, 
        // Iterates a point over its neighbors to approximate an iteration count.
        private PixelData<T> Mandelbrot<T, M>(
            T Zero, T OneHalf, T TwoPow8, // Constants
            T x0, T y0, List<GenericComplex<T>> iterList // Input values
            ) where M : IGenericMath<T>, new()
        {
            // Math objects
            M TMath = new M();
            ComplexMath<T, M> CMath = new ComplexMath<T, M>();

            // Get max iterations.  
            int maxIterations = iterList.Count;

            // Initialize our iteration count.
            int iterCount = 0;

            // Initialize some variables...
            GenericComplex<T> zn;

            GenericComplex<T> d0 = new GenericComplex<T>(x0, y0);

            GenericComplex<T> dn = d0;

            T znMagn = Zero;

            // Mandelbrot algorithm
            while (TMath.LessThan(znMagn, TwoPow8) && iterCount < maxIterations){

                // dn *= iter_list[iter] + dn
                dn = CMath.Multiply(dn, CMath.Add(iterList[iterCount], dn));

                // dn += d0
                dn = CMath.Add(dn, d0);

                // zn = x[iter] * 0.5 + dn
                zn = CMath.Add(CMath.Multiply(iterList[iterCount], OneHalf), dn);

                znMagn = CMath.MagnitudeSquared(zn);

                iterCount++;
            }

            return new PixelData<T>(znMagn, iterCount);
        }

        // Smooth Coloring Algorithm
        private Color GetColorFromIterationCount(int iterCount, double znMagn)
        {
            double temp_i = iterCount;
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(znMagn) / 2;
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

            // Fire frame start event
            FrameStart();

            int inSet = 0;

            // Initialize generic values
            T Zero = TMath.fromInt32(0);
            T OneHalf = TMath.fromDouble(0.5);
            T TwoPow8 = TMath.fromInt32(256);
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

            // x_min = -scaleFactor / zoom
            // x_max =  scaleFactor / zoom
            T xMin = TMath.Divide(TMath.Negate(scaleFactor), zoom);
            T xMax = TMath.Divide(scaleFactor, zoom);

            // y_min = -1 / zoom
            // y_max =  1 / zoom
            T yMin = TMath.Divide(TMath.fromInt32(-1), zoom);
            T yMax = TMath.Divide(TMath.fromInt32(1), zoom);

            var iterList = GetIterationList<T, M>(Zero, TwoPow10, NegTwoPow10, offsetX, offsetY);

            var loop = Parallel.For(0, Width, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                T x0 = Utils.Map<T, M>(TMath.fromInt32(px), Zero, FrameWidth, xMin, xMax);

                for (int py = 0; py < Height; py++)
                {
                    T y0 = Utils.Map<T, M>(TMath.fromInt32(py), Zero, FrameHeight, yMin, yMax);


                    PixelData<T> pixelData = Mandelbrot<T, M>(Zero, OneHalf, TwoPow8, x0, y0, iterList);

                    // Grab the values from our pixel data

                    T znMagn = pixelData.GetZnMagn();
                    int iterCount = pixelData.GetIterCount();

                    // if zn's magnitude surpasses the 
                    // bailout radius, give it a fancy color.
                    if (iterCount < iterList.Count) // itercount
                    {
                        Color PixelColor = GetColorFromIterationCount(iterCount, TMath.toDouble(znMagn));
                        CurrentFrame.SetPixel(px, py, PixelColor);
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        CurrentFrame.SetPixel(px, py, Color.Black);
                        Interlocked.Increment(ref inSet);
                    }
                }
            });

            if (inSet == Width * Height) StopRender();

            Bitmap newFrame = (Bitmap)CurrentFrame.Bitmap.Clone();
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
