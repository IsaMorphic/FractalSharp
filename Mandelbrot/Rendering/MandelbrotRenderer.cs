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
using ManagedCuda.BasicTypes;

namespace Mandelbrot.Rendering
{
    delegate void FrameStartDelegate();
    delegate void FrameStopDelegate(Bitmap frame);

    delegate void RenderStopDelegate();

    class MandelbrotRenderer
    {
        private CudaContext ctx;

        private int cell_x;
        private int cell_y;

        private bool Gradual = true;

        private GenericMathResolver MathResolver;
        private DirectBitmap CurrentFrame;

        private bool isInitialized = false;

        private int ThreadCount = Environment.ProcessorCount;
        public int MaxIterations { get; protected set; }
        public double Magnification { get; protected set; }

        protected decimal offsetXM;
        protected decimal offsetYM;
        private decimal aspectM;

        private int Width;
        private int Height;

        private int[] ChunkSizes = new int[12];
        private int[] MaxChunkSizes = new int[12];

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

            isInitialized = true;

            Setup(settings);
        }

        public void Setup(RenderSettings settings)
        {
            if (isInitialized)
            {
                bool hasChanged = (
                    offsetXM != settings.offsetX ||
                    offsetYM != settings.offsetY ||
                    Magnification != settings.Magnification ||
                    MaxIterations != settings.MaxIterations);

                Job = new CancellationTokenSource();

                offsetXM = settings.offsetX;
                offsetYM = settings.offsetY;

                Magnification = settings.Magnification;
                MaxIterations = settings.MaxIterations;

                ThreadCount = settings.ThreadCount;

                AlgorithmType = settings.AlgorithmType;

                Gradual = settings.Gradual;

                MaxChunkSizes = settings.MaxChunkSizes;

                if (hasChanged)
                {
                    for (var i = 0; i < ChunkSizes.Length; i++)
                    {
                        ChunkSizes[i] = MaxChunkSizes[i];
                    }
                }
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized!");
            }
        }

        public bool GPUAvailable()
        {
            bool cudaAvailable =
                CudaContext.GetDeviceCount() > 0 &&
                Environment.Is64BitOperatingSystem;

            return cudaAvailable;
        }

        public bool InitGPU()
        {
            if (isInitialized)
            {
                if (!GPUAvailable())
                    return false;

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

                GPUAlgorithmProvider.GPUInit(ctx, Resources.Kernel, new dim3(Width / 16, Height / 9), new dim3(4, 3));

                return true;
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized!");
            }
        }

        public void CleanupGPU()
        {
            try
            {
                ctx.Dispose();
            }
            catch (NullReferenceException) { }
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

        public void GetPointFromFrameLocation(int x, int y, out decimal offsetX, out decimal offsetY)
        {
            decimal xRange = aspectM / (decimal)Magnification;
            decimal yRange = 2 / (decimal)Magnification;
            offsetX = Utils.Map<decimal>(new DecimalMath(), x, 0, Width, -xRange + offsetXM, xRange + offsetXM);
            offsetY = Utils.Map<decimal>(new DecimalMath(), y, 0, Height, -yRange + offsetYM, yRange + offsetYM);
        }

        #endregion

        #region Rendering Methods


        protected async Task<int[]> RenderGPUCells()
        {
            ctx.SetCurrent();

            GPUAlgorithmProvider.GPUPreFrame();

            if (cell_x < 3) { cell_x++; }
            else if (cell_y < 2) { cell_x = 0; cell_y++; }
            else { cell_x = 0; cell_y = 0; }

            int cellWidth = Width / 4;
            int cellHeight = Height / 3;

            double xMax = (double)aspectM / Magnification;
            double yMax = 2 / Magnification;

            CudaDeviceVariable<int> dev_palette = int_palette;
            CudaDeviceVariable<int> dev_image = CurrentFrame.Bits;

            bool exit = false;
            for (int tmpcell_x = 0; tmpcell_x < 4 && !exit; tmpcell_x++)
            {
                for (int tmpcell_y = 0; tmpcell_y < 3 && !exit; tmpcell_y++)
                {
                    if (!Gradual)
                    {
                        cell_x = tmpcell_x;
                        cell_y = tmpcell_y;
                    }
                    else
                    {
                        exit = true;
                    }

                    int index = cell_x + cell_y * 4;
                    int chunkSize = ChunkSizes[index];
                    int maxChunkSize = MaxChunkSizes[index];

                    GPUAlgorithmProvider.GPUCell(
                        dev_image,
                        dev_palette,
                        cell_x, cell_y,
                        cellWidth, cellHeight,
                        4, 3,
                        xMax, yMax,
                        chunkSize, maxChunkSize);

                    if (chunkSize > 1)
                        ChunkSizes[index] = chunkSize / 2;
                }
            }

            int[] raw_image = dev_image;

            GPUAlgorithmProvider.GPUPostFrame();

            dev_palette.Dispose();
            dev_image.Dispose();

            return raw_image;
        }

        public void RenderFrameGPU()
        {
            FrameStart();

            IGenericMath<double> TMath = MathResolver.CreateMathObject<double>();
            GPUAlgorithmProvider.Init(TMath, offsetXM, offsetYM, MaxIterations);

            var renderTask = Task.Run(RenderGPUCells, Job.Token);

            renderTask.Wait();

            int[] raw_image = renderTask.Result;

            CurrentFrame.SetBits(raw_image);

            Bitmap NewFrame = (Bitmap)CurrentFrame.Bitmap.Clone();

            FrameEnd(NewFrame);
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame<T>()
        {
            Type NumType = typeof(T);

            // Initialize Math Object
            IGenericMath<T> TMath = MathResolver.CreateMathObject<T>();

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

            algorithmProvider.Init(TMath, offsetXM, offsetYM, MaxIterations);

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
