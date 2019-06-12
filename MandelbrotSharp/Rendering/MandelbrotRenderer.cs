/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
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
        public event EventHandler<FrameEventArgs> FrameFinished;

        public TaskStatus? RenderStatus => RenderTask?.Status;

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

        protected Gradient OuterColors { get; private set; }
        protected RgbaValue InnerColor { get; private set; }

        protected Type AlgorithmType { get; private set; }
        protected Type ArithmeticType { get; private set; }
        protected Type PixelColoratorType { get; private set; }

        protected Dictionary<string, object> ExtraParams { get; private set; }

        private CancellationTokenSource TokenSource;
        private Task AlgorithmInitTask;
        private Task RenderTask;

        protected virtual void OnRenderHalted()
        {
            RenderHalted?.Invoke(this, null);
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

        public MandelbrotRenderer(int width, int height)
        {
            Width = width;
            Height = height;

            aspectRatio = ((BigDecimal)Width / (BigDecimal)Height) * 2;

            CurrentFrame = new RgbaImage(Width, Height);
        }

        public void Setup(RenderSettings settings)
        {
            TokenSource = new CancellationTokenSource();

            offsetX = settings.offsetX;
            offsetY = settings.offsetY;

            Magnification = settings.Magnification;
            MaxIterations = settings.MaxIterations;

            ThreadCount = settings.ThreadCount;

            AlgorithmType = settings.AlgorithmType;

            ArithmeticType = settings.ArithmeticType;

            PixelColoratorType = settings.PixelColoratorType;

            OuterColors = settings.OuterColors;

            PixelColorator = (PixelColorator)Activator.CreateInstance(PixelColoratorType);

            ExtraParams = new Dictionary<string, object>(settings.ExtraParams);

            var genericType = typeof(PointMapper<>).MakeGenericType(ArithmeticType);
            PointMapper = (IPointMapper)Activator.CreateInstance(genericType);

            PointMapper.SetInputSpace(0, Width, 0, Height);

            genericType = AlgorithmType.MakeGenericType(ArithmeticType);
            AlgorithmProvider = (IAlgorithmProvider)Activator.CreateInstance(genericType);
            UpdateAlgorithmProvider();

            Configure(settings);
        }

        protected virtual void Configure(RenderSettings settings) { }

        protected void UpdateAlgorithmProvider()
        {
            AlgorithmProvider.UpdateParams(new AlgorithmParams
            {
                Magnification = Magnification,
                offsetX = offsetX,
                offsetY = offsetY,
                MaxIterations = MaxIterations,
                ExtraParams = new Dictionary<string, object>(ExtraParams)
            });
            AlgorithmInitTask = Task.Factory.StartNew(
                AlgorithmProvider.Initialize, 
                TokenSource.Token, 
                TokenSource.Token);
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

        public Task StartRenderFrame()
        {
            if (RenderStatus == TaskStatus.Running)
                throw new Exception("The running task has not yet completed.");
            RenderTask = Task.Factory.StartNew(RenderFrame, TokenSource.Token);
            return RenderTask.ContinueWith(RenderTaskFinished);
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        private void RenderFrame()
        {
            // Fire frame start event
            OnFrameStarted();

            UpdatePointMapperOutputSpace();

            var firstPoint = GetFrameFirstPixel();
            var lastPoint = GetFrameLastPixel();

            var options = new ParallelOptions { MaxDegreeOfParallelism = ThreadCount };

            AlgorithmInitTask.Wait();

            Parallel.For(firstPoint.Y, lastPoint.Y, options, py =>
            {
                if (ShouldSkipRow(py))
                    return;
                var y0 = PointMapper.MapPointY(py);
                Parallel.For(firstPoint.X, lastPoint.X, options, px =>
                {
                    var p = new Pixel(px, py);
                    if (ShouldSkipPixel(p))
                        return;

                    var x0 = PointMapper.MapPointX(px);

                    PixelData pixelData = AlgorithmProvider.Run(x0, y0);

                    if (pixelData.Escaped)
                    {
                        WritePixelToFrame(p, InnerColor);
                    }
                    else
                    {
                        double colorIndex = PixelColorator.GetIndexFromPixelData(pixelData);

                        WritePixelToFrame(p, OuterColors[colorIndex]);
                    }
                    TokenSource.Token.ThrowIfCancellationRequested();
                });
            });
        }

        private void RenderTaskFinished(Task task)
        {
            if (task != RenderTask)
                throw new Exception("This is not the task you are looking for...");
            RenderTask.Dispose();
            if (RenderStatus == TaskStatus.Canceled)
            {
                TokenSource.Dispose();
                TokenSource = new CancellationTokenSource();
                OnRenderHalted();
            }
            else
            {
                RgbaImage copy = new RgbaImage(CurrentFrame);
                OnFrameFinished(new FrameEventArgs(copy));
            }
        }

        public void StopRenderFrame()
        {
            if (RenderStatus == TaskStatus.Running)
                TokenSource.Cancel();
            else
                throw new Exception("There is no running task to cancel");
        }
        #endregion
    }
}
