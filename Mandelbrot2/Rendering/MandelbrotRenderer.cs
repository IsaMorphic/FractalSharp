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

        public int NumFrames { get; private set; }
        public long MaxIterations { get; private set; }
        public double Magnification { get; private set; }


        private decimal offsetXM;
        private decimal offsetYM;

        private Quadruple offsetXQ;
        private Quadruple offsetYQ;
        private Quadruple aspectQ;

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

            Magnification = 1;

            aspectQ = (double)Width / (double)Height;

            currentFrame = new DirectBitmap(Width, Height);

            palette = newPalette;

        }

        public void Setup(RenderSettings settings)
        {
            Job = new CancellationTokenSource();

            offsetXM = settings.offsetX;
            offsetYM = settings.offsetY;

            offsetXQ = Quadruple.FromString(offsetXM.ToString());
            offsetYQ = Quadruple.FromString(offsetYM.ToString());

            MaxIterations = settings.MaxIterations;
            NumFrames = settings.NumFrames;
            ThreadCount = settings.ThreadCount;
        }

        #endregion

        #region Algorithm Methods
        // Traditional Mandelbrot algorithm, 
        // using generic typing in order to increase modularity
        private void mandelbrot<T, M>
            (T Zero, T Two, T Four, T x0, T y0, out T xx, out T yy, out long iter)
            where M : IGenericMath<T>, new()
        {
            M TMath = new M();

            // Initialize some variables..
            T x = Zero;
            T y = Zero;

            // Define x squared and y squared as their own variables
            // To avoid unnecisarry multiplication.
            xx = Zero;
            yy = Zero;

            // Initialize our iteration count.
            iter = 0;

            // Mandelbrot algorithm
            while (TMath.LessThan(TMath.Add(xx, yy), Four) && iter < MaxIterations)
            {
                // xtemp = xx - yy + x0
                T xtemp = TMath.Add(TMath.Subtract(xx, yy), x0);
                // ytemp = 2 * x * y + y0
                T ytemp = TMath.Add(TMath.Multiply(Two, TMath.Multiply(x, y)), y0);

                if (TMath.EqualTo(x, xtemp) && TMath.EqualTo(y, ytemp))
                {
                    iter = MaxIterations;
                    break;
                }

                x = xtemp;
                y = ytemp;
                xx = TMath.Multiply(x, x);
                yy = TMath.Multiply(y, y);

                iter++;
            }

        }

        // Smooth Coloring Algorithm
        private Color GetColorFromIterationCount(long iterations, double xx, double yy)
        {
            double temp_i = iterations;
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(xx + yy) / 2;
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

        public void SetFrame(int frameNum)
        {
            // Set variables and get new zoom value.  
            NumFrames = frameNum;

            Magnification = Math.Pow(NumFrames, NumFrames / 100.0);

            MaxIterations += NumFrames / Math.Max(5 - NumFrames / 100, 1);

        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame<T, M>() where M : IGenericMath<T>, new()
        {
            M TMath = new M();

            // Fire frame start event
            FrameStart();

            long in_set = 0;

            // Initialize generic values
            T Zero = TMath.fromInt32(0);
            T Two = TMath.fromInt32(2);
            T Four = TMath.fromInt32(4);

            // Cast type specific values to the generic type
            T FrameWidth = TMath.fromInt32(Width);
            T FrameHeight = TMath.fromInt32(Height);

            T zoom = TMath.fromDouble(Magnification);

            T offsetX = TMath.fromQuadruple(offsetXQ);
            T offsetY = TMath.fromQuadruple(offsetYQ);

            T scaleFactor = TMath.fromQuadruple(aspectQ);

            // Predefine minimum and maximum values of the plane, 
            // In order to avoid making unnecisary calculations on each pixel.  

            // x_min = -scaleFactor / zoom + offsetX
            // x_max =  scaleFactor / zoom + offsetX
            T x_min = TMath.Add(TMath.Divide(TMath.Negate(scaleFactor), zoom), offsetX);
            T x_max = TMath.Add(TMath.Divide(scaleFactor, zoom), offsetX);

            // y_min = -1 / zoom + offsetY
            // y_max =  1 / zoom + offsetY
            T y_min = TMath.Add(TMath.Divide(TMath.fromInt32(-1), zoom), offsetY);
            T y_max = TMath.Add(TMath.Divide(TMath.fromInt32(1), zoom), offsetY);

            var loop = Parallel.For(0, Width, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                T x0 = Utils.Map<T, M>(TMath.fromInt32(px), Zero, FrameWidth, x_min, x_max);

                for (int py = 0; py < Height; py++)
                {
                    T y0 = Utils.Map<T, M>(TMath.fromInt32(py), Zero, FrameHeight, y_min, y_max);

                    // Define x squared and y squared as their own variables
                    // To avoid unnecisarry multiplication.
                    T xx = Zero;
                    T yy = Zero;

                    // Initialize our iteration count.
                    long iteration = 0;

                    mandelbrot<T, M>(Zero, Two, Four, x0, y0, out xx, out yy, out iteration);

                    // If x squared plus y squared is outside the set, give it a fancy color.
                    if (TMath.GreaterThan(TMath.Add(xx, yy), Four)) // xx + yy > 4
                    {
                        Color PixelColor = GetColorFromIterationCount(iteration, TMath.toDouble(xx), TMath.toDouble(yy));
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
