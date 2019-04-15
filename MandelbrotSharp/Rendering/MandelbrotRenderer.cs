using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
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

    public class ConfigEventArgs : EventArgs
    {
        public ConfigEventArgs(RenderSettings settings)
        {
            Settings = settings;
        }

        public RenderSettings Settings;
    }

    public class MandelbrotRenderer
    {
        public event EventHandler FrameStarted;
        public event EventHandler<ConfigEventArgs> ConfigurationUpdated;
        public event EventHandler<FrameEventArgs> FrameFinished;

        protected int MaxIterations;
        protected BigDecimal Magnification;
        protected BigDecimal offsetX;
        protected BigDecimal offsetY;

        protected int Width { get; private set; }
        protected int Height { get; private set; }

        protected int ThreadCount { get; private set; }

        protected dynamic AlgorithmProvider { get; private set; }
        protected dynamic PointMapper { get; private set; }

        protected RgbaImage CurrentFrame { get; private set; }

        protected BigDecimal aspectRatio { get; private set; }

        private Type AlgorithmType;
        private Type ArithmeticType;

        private CancellationTokenSource CancelTokenSource;

        private bool isInitialized = false;

        protected virtual void OnConfigurationUpdated(ConfigEventArgs e)
        {
            ConfigurationUpdated?.Invoke(this, e);
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

            Setup(settings);
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

                dynamic TMath = GenericMathResolver.CreateMathObject(ArithmeticType);

                var genericType = typeof(PointMapper<>).MakeGenericType(ArithmeticType);
                PointMapper = Activator.CreateInstance(genericType, TMath);

                PointMapper.SetInputSpace(0, Width, 0, Height);

                genericType = AlgorithmType.MakeGenericType(ArithmeticType);
                AlgorithmProvider = Activator.CreateInstance(genericType, TMath);

                UpdateAlgorithmProvider();

                OnConfigurationUpdated(new ConfigEventArgs(settings));
            }
            else
            {
                throw new ApplicationException("Renderer is not Initialized!");
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
                Token = CancelTokenSource.Token
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

        protected virtual Pixel GetFrameFirstPixel()
        {
            return new Pixel(0, 0);
        }

        protected virtual Pixel GetFrameLastPixel()
        {
            return new Pixel(Width, Height);
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

            var loop = Parallel.For(firstPoint.X, lastPoint.X, new ParallelOptions { CancellationToken = CancelTokenSource.Token, MaxDegreeOfParallelism = ThreadCount }, px =>
            {
                var x0 = PointMapper.MapPointX(px);
                for (int py = firstPoint.Y; py < lastPoint.Y; py++)
                {
                    var y0 = PointMapper.MapPointY(py);
                    var p = new Pixel(px, py);

                    if (ShouldSkipPixel(p))
                        continue;

                    PixelData pixelData = AlgorithmProvider.Run(x0, y0);

                    RgbaValue PixelColor = GetColorFromPixelData(pixelData);

                    WritePixelToFrame(p, PixelColor);
                }
            });

            OnFrameFinished(new FrameEventArgs(new RgbaImage(CurrentFrame)));
        }

        // Method that signals the render process to stop.  
        public void StopRender()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource = new CancellationTokenSource();
        }

        #endregion
    }
}
