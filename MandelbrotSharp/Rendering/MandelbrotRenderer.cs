using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Utilities;
using System;
using System.Collections.Generic;
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
        public event EventHandler FrameStarted;
        public event EventHandler RenderHalted;
        public event EventHandler ConfigurationUpdated;
        public event EventHandler<FrameEventArgs> FrameFinished;

        protected int MaxIterations;
        protected BigDecimal Magnification;
        protected BigDecimal offsetX;
        protected BigDecimal offsetY;

        protected int Width { get; private set; }
        protected int Height { get; private set; }

        protected BigDecimal aspectRatio { get; private set; }

        protected int ThreadCount { get; private set; }

        protected IAlgorithmProvider AlgorithmProvider { get; private set; }
        protected IPointMapper PointMapper { get; private set; }

        protected PixelColorator PixelColorator { get; private set; }

        protected RgbaImage CurrentFrame { get; private set; }

        protected RgbaValue[] Palette { get; private set; }

        protected Type AlgorithmType { get; private set; }
        protected Type ArithmeticType { get; private set; }
        protected Type PixelColoratorType { get; private set; }

        protected Dictionary<string, object> ExtraParams { get; private set; }

        private CancellationTokenSource CancelTokenSource;

        private bool isInitialized = false;

        protected virtual void OnRenderHalted()
        {
            RenderHalted?.Invoke(this, null);
        }

        protected virtual void OnConfigurationUpdated()
        {
            ConfigurationUpdated?.Invoke(this, null);
        }

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

            aspectRatio = ((BigDecimal)Width / (BigDecimal)Height) * 2;

            CurrentFrame = new RgbaImage(Width, Height);

            isInitialized = true;
        }

        public void Setup(RenderSettings settings)
        {
            if (isInitialized)
            {
                CancelTokenSource = new CancellationTokenSource();

                offsetX = settings.offsetX;
                offsetY = settings.offsetY;

                Magnification = settings.Magnification;
                MaxIterations = settings.MaxIterations;

                ThreadCount = settings.ThreadCount;

                AlgorithmType = settings.AlgorithmType;

                ArithmeticType = settings.ArithmeticType;

                PixelColoratorType = settings.PixelColoratorType;

                Palette = (RgbaValue[])settings.Palette.Clone();

                PixelColorator = (PixelColorator)Activator.CreateInstance(PixelColoratorType);

                ExtraParams = new Dictionary<string, object>(settings.ExtraParams);

                var genericType = typeof(PointMapper<>).MakeGenericType(ArithmeticType);
                PointMapper = (IPointMapper)Activator.CreateInstance(genericType);

                PointMapper.SetInputSpace(0, Width, 0, Height);

                genericType = AlgorithmType.MakeGenericType(ArithmeticType);
                AlgorithmProvider = (IAlgorithmProvider)Activator.CreateInstance(genericType);
                AlgorithmProvider.ParamsUpdated += (s, e) => OnConfigurationUpdated();
                UpdateAlgorithmProvider();
            }
            else
            {
                throw new Exception("Renderer is not Initialized!");
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
                Token = CancelTokenSource.Token,
                ExtraParams = new Dictionary<string, object>(ExtraParams)
        });
        }

        #endregion

        #region Algorithm Methods

        protected virtual Pixel GetFrameFirstPixel()
        {
            return new Pixel(0, 0);
        }

        protected virtual Pixel GetFrameLastPixel()
        {
            return new Pixel(Width, Height);
        }

        protected virtual bool ShouldSkipRow(int y)
        {
            return false;
        }

        protected virtual bool ShouldSkipPixel(Pixel p)
        {
            return false;
        }

        protected virtual void WritePixelToFrame(Pixel p, RgbaValue color)
        {
            CurrentFrame.SetPixel(p.X, p.Y, color);
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

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        public void RenderFrame()
        {
            // Fire frame start event
            OnFrameStarted();

            UpdatePointMapperOutputSpace();

            var firstPoint = GetFrameFirstPixel();
            var lastPoint = GetFrameLastPixel();

            var loop = Parallel.For(firstPoint.Y, lastPoint.Y, new ParallelOptions { CancellationToken = CancelTokenSource.Token, MaxDegreeOfParallelism = ThreadCount }, py =>
            {
                if (ShouldSkipRow(py))
                    return;
                var y0 = PointMapper.MapPointY(py);
                Parallel.For(firstPoint.X, lastPoint.X, new ParallelOptions { CancellationToken = CancelTokenSource.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
                {
                    var p = new Pixel(px, py);
                    if (ShouldSkipPixel(p))
                        return;

                    var x0 = PointMapper.MapPointX(px);

                    PixelData pixelData = AlgorithmProvider.Run(x0, y0);

                    double colorIndex = PixelColorator.GetPaletteIndexFromPixelData(pixelData);

                    // Grab two colors from the pallete
                    RgbaValue color1 = Palette[(int)colorIndex % (Palette.Length - 1)];
                    RgbaValue color2 = Palette[(int)(colorIndex + 1) % (Palette.Length - 1)];

                    // Lerp between both colors
                    RgbaValue final = RgbaValue.LerpColors(color1, color2, colorIndex % 1);

                    WritePixelToFrame(p, final);
                });
            });

            OnFrameFinished(new FrameEventArgs(new RgbaImage(CurrentFrame)));
        }

        // Method that signals the render process to stop.  
        public void StopRender()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource = new CancellationTokenSource();
            OnRenderHalted();
        }

        #endregion
    }
}
