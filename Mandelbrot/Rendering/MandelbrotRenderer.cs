using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ManagedCuda;
using ManagedCuda.VectorTypes;

using Mandelbrot.Imaging;
using Mandelbrot.Mathematics;
using Mandelbrot.Utilities;
using Mandelbrot.Algorithms;
using System.IO;
using Mandelbrot.Properties;

namespace Mandelbrot.Rendering
{
    delegate void FrameStartDelegate();
    delegate void FrameStopDelegate(Bitmap frame);

    delegate void RenderStopDelegate();

    class MandelbrotRenderer
    {
        private CudaContext ctx;

        private GenericMathResolver MathResolver;

        private DirectBitmap CurrentFrame;

        private int ThreadCount = Environment.ProcessorCount;
        public int MaxIterations { get; protected set; }
        public double Magnification { get; protected set; }

        protected decimal offsetXM;
        protected decimal offsetYM;
        private decimal aspectM;

        private int Width;
        private int Height;

        private RGB[] palette;
        private int[] int_palette;

        private Type AlgorithmType;
        private IAlgorithmProvider<double> GPUAlgorithmProvider;

        private CancellationTokenSource Job;

        public event FrameStartDelegate FrameStart;
        public event FrameStopDelegate FrameEnd;
        public event RenderStopDelegate RenderHalted;

        #region Initialization and Configuration Methods

        public void Initialize(RenderSettings settings, RGB[] newPalette, GenericMathResolver mathResolver)
        {
            MathResolver = mathResolver;

            Width = settings.Width;
            Height = settings.Height;

            aspectM = ((decimal)Width / (decimal)Height) * 2;

            CurrentFrame = new DirectBitmap(Width, Height);

            palette = newPalette;

            Setup(settings);
        }

        public void Setup(RenderSettings settings)
        {
            Job = new CancellationTokenSource();

            offsetXM = settings.offsetX;
            offsetYM = settings.offsetY;

            Magnification = settings.Magnification;
            MaxIterations = settings.MaxIterations;

            ThreadCount = settings.ThreadCount;

            AlgorithmType = settings.AlgorithmType;
        }

        #endregion

        #region Algorithm Methods

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

        public void InitGPU()
        {
            ctx = new CudaContext(CudaContext.GetMaxGflopsDeviceId());

            int_palette = new int[palette.Length];
            for (var i = 0; i < palette.Length; i++)
            {
                int_palette[i] = palette[i].toColor().ToArgb();
            }

            Type algorithmType = AlgorithmType.MakeGenericType(typeof(double));

            GPUAlgorithmProvider =
                (IAlgorithmProvider<double>)Activator
                .CreateInstance(algorithmType);

            GPUAlgorithmProvider.GPUInit(ctx);
        }

        public void RenderFrameGPU()
        {
            FrameStart();

            double xMax = (double)aspectM / Magnification;
            double yMax = 2 / Magnification;

            int[] raw_image = GPUAlgorithmProvider.GPUFrame(
                int_palette, Width, Height, 
                xMax, yMax, 
                (double)offsetXM, 
                (double)offsetYM, 
                MaxIterations);

            CurrentFrame.SetBits(raw_image);

            Bitmap NewFrame = (Bitmap)CurrentFrame.Bitmap.Clone();

            FrameEnd(NewFrame);
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame<T>()
        {
            Type NumType;

            // Initialize Math Object
            IGenericMath<T> TMath = MathResolver.CreateMathObject<T>(out NumType);

            // Initialize Algorithm Provider
            Type algorithmType = AlgorithmType.MakeGenericType(NumType);

            IAlgorithmProvider<T> algorithmProvider =
                (IAlgorithmProvider<T>)Activator
                .CreateInstance(algorithmType);

            // Fire frame start event
            FrameStart();

            int in_set = 0;

            T Zero = TMath.fromInt32(0);

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
            T yMin = TMath.Divide(TMath.fromInt32(-2), zoom);
            T yMax = TMath.Divide(TMath.fromInt32(2), zoom);

            algorithmProvider.Init(TMath, offsetX, offsetY, MaxIterations);

            var loop = Parallel.For(0, Width, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                T x0 = Utils.Map<T>(TMath, TMath.fromInt32(px), Zero, FrameWidth, xMin, xMax);

                for (int py = 0; py < Height; py++)
                {
                    T y0 = Utils.Map<T>(TMath, TMath.fromInt32(py), Zero, FrameHeight, yMin, yMax);


                    PixelData<T> pixelData = algorithmProvider.Run(x0, y0);

                    // Grab the values from our pixel data

                    T znMagn = pixelData.GetZnMagn();
                    int iterCount = pixelData.GetIterCount();
                    bool isBelowMaxIter = pixelData.GetBelowMaxIter();

                    // if zn's magnitude surpasses the 
                    // bailout radius, give it a fancy color.
                    if (isBelowMaxIter) // itercount
                    {
                        Color PixelColor = GetColorFromIterationCount(iterCount, TMath.toDouble(znMagn));
                        CurrentFrame.SetPixel(px, py, PixelColor);
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        CurrentFrame.SetPixel(px, py, Color.Black);
                        Interlocked.Increment(ref in_set);
                    }
                }
            });

            if (in_set == Width * Height) StopRender();

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
