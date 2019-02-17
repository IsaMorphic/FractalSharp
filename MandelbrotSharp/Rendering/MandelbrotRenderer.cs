using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using MandelbrotSharp.Utilities;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public delegate void FrameStartDelegate();
    public delegate void FrameStopDelegate(Bitmap frame);

    public delegate void RenderStopDelegate();

    public class MandelbrotRenderer
    { 

        private int CellX;
        private int CellY;

        private bool Gradual = true;


        private GenericMathResolver MathResolver;
        private DirectBitmap CurrentFrame;
        private dynamic AlgorithmProvider, PointMapper;

        private bool isInitialized = false;

        private int ThreadCount = Environment.ProcessorCount;

        protected int MaxIterations;
        protected BigDecimal Magnification;
        protected BigDecimal offsetX;
        protected BigDecimal offsetY;

        private BigDecimal aspectRatio;

        private int Width;
        private int Height;

        private int TotalCellsX = 4;
        private int TotalCellsY = 3;

        private int CellWidth;
        private int CellHeight;

        private int[] ChunkSizes = new int[12];
        private int[] MaxChunkSizes = new int[12];

        private RGB[] palette;

        private Type AlgorithmType;
        private Type ArithmeticType;

        private CancellationTokenSource Job;

        public event FrameStartDelegate FrameStarted;
        public event FrameStopDelegate FrameFinished;
        public event RenderStopDelegate RenderHalted;

        protected virtual void FrameStart()
        {
            FrameStarted();
        }

        protected virtual void FrameEnd(Bitmap frame)
        {
            FrameFinished(frame);
        }

        #region Initialization and Configuration Methods

        public void Initialize(RenderSettings settings, RGB[] newPalette, GenericMathResolver mathResolver)
        {
            MathResolver = mathResolver;

            Width = settings.Width;
            Height = settings.Height;

            CellWidth = Width / TotalCellsX;
            CellHeight = Height / TotalCellsY;

            aspectRatio = ((BigDecimal)Width / (BigDecimal)Height) * 2;

            CurrentFrame = new DirectBitmap(Width, Height);

            palette = newPalette;

            isInitialized = true;

            Setup(settings);
        }

        public void Setup(RenderSettings settings)
        {
            if (isInitialized)
            {
                Job = new CancellationTokenSource();

                offsetX = settings.offsetX;
                offsetY = settings.offsetY;

                Magnification = settings.Magnification;
                MaxIterations = settings.MaxIterations;

                ThreadCount = settings.ThreadCount;

                AlgorithmType = settings.AlgorithmType;

                ArithmeticType = settings.ArithmeticType;

                Gradual = settings.Gradual;

                MaxChunkSizes = settings.MaxChunkSizes;

                ResetChunkSizes();

                dynamic TMath = MathResolver.CreateMathObject(ArithmeticType);

                var genericType = typeof(PointMapper<>).MakeGenericType(ArithmeticType);
                PointMapper = Activator.CreateInstance(genericType, TMath);

                PointMapper.SetInputSpace(0, Width, 0, Height);

                genericType = AlgorithmType.MakeGenericType(ArithmeticType);
                AlgorithmProvider = Activator.CreateInstance(genericType, TMath);

                UpdateAlgorithmProvider();
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized!");
            }
        }

        protected void ResetChunkSizes() {
            for (var i = 0; i < ChunkSizes.Length; i++)
            {
                ChunkSizes[i] = MaxChunkSizes[i];
            }
        }

        protected void UpdateAlgorithmProvider()
        {
            AlgorithmProvider.UpdateParams(new AlgorithmParams
            {
                Magnification = Magnification,
                offsetX = offsetX,
                offsetY = offsetY,
                MaxIterations = MaxIterations,
                Token = Job.Token
            });
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

        public void GetPointFromFrameLocation(int x, int y, out BigDecimal offsetX, out BigDecimal offsetY)
        {
            BigDecimal xRange = aspectRatio / Magnification;
            BigDecimal yRange = 2 / Magnification;
            offsetX = Utils.Map<BigDecimal>(new BigDecimalMath(), x, 0, Width, -xRange + this.offsetX, xRange + this.offsetX);
            offsetY = Utils.Map<BigDecimal>(new BigDecimalMath(), y, 0, Height, -yRange + this.offsetY, yRange + this.offsetY);
        }

        #endregion

        #region Rendering Methods

        protected void IncrementCellCoords()
        {
            if (CellX < TotalCellsX - 1) { CellX++; }
            else if (CellY < TotalCellsY - 1) { CellX = 0; CellY++; }
            else { CellX = 0; CellY = 0; }
        }

        public void RenderCell()
        {
            int in_set = 0;

            int index = CellX + CellY * 4;
            int chunkSize = ChunkSizes[index];
            int maxChunkSize = MaxChunkSizes[index];

            BigDecimal scaleFactor = aspectRatio;
            BigDecimal zoom = Magnification;
            // Predefine minimum and maximum values of the plane, 
            // In order to avoid making unnecisary calculations on each pixel.  

            // x_min = -scaleFactor / zoom
            // x_max =  scaleFactor / zoom
            BigDecimal xMin = -scaleFactor / zoom + offsetX;
            BigDecimal xMax = scaleFactor / zoom + offsetX;

            // y_min = -2 / zoom
            // y_max =  2 / zoom
            BigDecimal yMin = -2 / zoom + offsetY;
            BigDecimal yMax = 2 / zoom + offsetY;

            PointMapper.SetOutputSpace(xMin, xMax, yMin, yMax);

            var loop = Parallel.For(CellX * CellWidth, (CellX + 1) * CellWidth, new ParallelOptions { CancellationToken = Job.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                var x0 = PointMapper.MapPointX(px);
                for (int py = CellY * CellHeight; py < (CellY + 1) * CellHeight; py++)
                {
                    var y0 = PointMapper.MapPointY(py);
                    if ((px % chunkSize != 0 ||
                         py % chunkSize != 0) ||
                       ((px / chunkSize) % 2 == 0 &&
                        (py / chunkSize) % 2 == 0 &&
                        maxChunkSize != chunkSize))
                        continue;

                    PixelData pixelData = AlgorithmProvider.Run(x0, y0);

                    // Grab the values from our pixel data

                    double magn = pixelData.GetMagnitude();
                    int iterCount = pixelData.GetIterCount();
                    bool pointEscaped = pixelData.GetEscaped();

                    Color PixelColor;

                    // if zn's magnitude surpasses the 
                    // bailout radius, give it a fancy color.
                    if (pointEscaped) // itercount
                    {
                        PixelColor = GetColorFromIterationCount(iterCount, magn);
                    }
                    // Otherwise, make the pixel black, as it is in the set.  
                    else
                    {
                        PixelColor = Color.Black;
                        Interlocked.Increment(ref in_set);
                    }

                    for (var i = px; i < px + chunkSize; i++)
                    {
                        for (var j = py; j < py + chunkSize; j++)
                        {
                            if (i < Width && j < Height)
                            {
                                CurrentFrame.SetPixel(i, j, PixelColor);
                            }
                        }
                    }
                }
            });

            if (chunkSize > 1)
                ChunkSizes[index] /= 2;

            if (in_set == Width * Height) StopRender();
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame()
        {
            // Fire frame start event
            FrameStart();

            if (Gradual)
            {
                IncrementCellCoords();
                RenderCell();
            }
            else
            {
                for (CellX = 0; CellX < TotalCellsX; CellX++)
                {
                    for (CellY = 0; CellY < TotalCellsY; CellY++)
                    {
                        RenderCell();
                    }
                }
            }

            Bitmap newFrame = new Bitmap(CurrentFrame.Bitmap);
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
