using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Utilities;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    // Define a class to hold custom event info
    public class FrameEventArgs : EventArgs
    {
        public FrameEventArgs(RgbaImage frame)
        {
            Frame = frame;
        }

        public RgbaImage Frame;
    }

    public class MandelbrotRenderer
    {

        private int CellX;
        private int CellY;

        private bool Gradual = true;

        protected RgbaImage CurrentFrame { get; private set; }

        private dynamic AlgorithmProvider;
        protected dynamic PointMapper;

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

        private Type AlgorithmType;
        private Type ArithmeticType;

        private CancellationTokenSource Job;

        public event EventHandler FrameStarted;
        public event EventHandler<FrameEventArgs> FrameFinished;

        protected virtual void OnFrameStarted()
        {
            FrameStarted?.Invoke(this, null);
        }

        protected virtual void OnFrameFinished(FrameEventArgs e)
        {
            FrameFinished?.Invoke(this, e);
        }

        #region Initialization and Configuration Methods

        public void Initialize(RenderSettings settings)
        {
            Width = settings.Width;
            Height = settings.Height;

            CellWidth = Width / TotalCellsX;
            CellHeight = Height / TotalCellsY;

            aspectRatio = ((BigDecimal)Width / (BigDecimal)Height) * 2;

            CurrentFrame = new RgbaImage(Width, Height);

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

                dynamic TMath = GenericMathResolver.CreateMathObject(ArithmeticType);

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

        protected void ResetChunkSizes()
        {
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
        protected virtual RgbaValue GetColorFromPixelData(PixelData data)
        {
            if (data.Escaped)
                return new RgbaValue(0,0,0);
            else
                return new RgbaValue(200, 200, 200);
        }

        #endregion

        #region Rendering Methods

        protected void UpdatePointMapperOutputSpace()
        {
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
        }

        protected void IncrementCellCoords()
        {
            if (CellX < TotalCellsX - 1) { CellX++; }
            else if (CellY < TotalCellsY - 1) { CellX = 0; CellY++; }
            else { CellX = 0; CellY = 0; }
        }

        public void RenderCell()
        {
            int index = CellX + CellY * 4;
            int chunkSize = ChunkSizes[index];
            int maxChunkSize = MaxChunkSizes[index];

            UpdatePointMapperOutputSpace();

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

                    RgbaValue PixelColor = GetColorFromPixelData(pixelData);

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
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame()
        {
            // Fire frame start event
            OnFrameStarted();

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
            OnFrameFinished(new FrameEventArgs(new RgbaImage(CurrentFrame)));
        }

        // Method that signals the render process to stop.  
        public void StopRender()
        {
            Job.Cancel();
            Job = new CancellationTokenSource();
        }

        #endregion
    }
}
