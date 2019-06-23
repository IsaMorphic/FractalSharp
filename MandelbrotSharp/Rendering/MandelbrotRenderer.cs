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
using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;
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
        protected Complex<BigDecimal> Location;

        protected int Width { get; private set; }
        protected int Height { get; private set; }

        protected BigDecimal aspectRatio { get; private set; }

        protected int ThreadCount { get; private set; }

        protected IAlgorithmProvider AlgorithmProvider { get; private set; }
        protected IPointMapper PointMapper { get; private set; }

        protected PointColorer PointColorer { get; private set; }

        protected RgbaImage CurrentFrame { get; private set; }

        protected Gradient OuterColors { get; private set; }
        protected RgbaValue InnerColor { get; private set; }

        protected Type AlgorithmType { get; private set; }
        protected Type ArithmeticType { get; private set; }
        protected Type PointColorerType { get; private set; }

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

            Location = settings.Location;

            Magnification = settings.Magnification;
            MaxIterations = settings.MaxIterations;

            ThreadCount = settings.ThreadCount;

            AlgorithmType = settings.AlgorithmType;

            ArithmeticType = settings.ArithmeticType;

            PointColorerType = settings.PointColorerType;

            OuterColors = settings.OuterColors;

            InnerColor = settings.InnerColor;

            PointColorer = (PointColorer)Activator.CreateInstance(PointColorerType);

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
                Location = Location,
                MaxIterations = MaxIterations,
                ExtraParams = new Dictionary<string, object>(ExtraParams)
            });
            AlgorithmInitTask = Task.Factory.StartNew(
                AlgorithmProvider.Initialize, 
                TokenSource.Token, 
                TokenSource.Token);
        }

        #endregion

        #region Rendering Methods

        protected virtual IntPoint GetFrameFirstPixel()
        {
            return new IntPoint(0, 0);
        }

        protected virtual IntPoint GetFrameLastPixel()
        {
            return new IntPoint(Width, Height);
        }

        protected virtual bool ShouldSkipRow(int y)
        {
            return false;
        }

        protected virtual bool ShouldSkipPixel(IntPoint p)
        {
            return false;
        }

        protected virtual void WritePixelToFrame(IntPoint p, RgbaValue color)
        {
            CurrentFrame.SetPixel(p.X, p.Y, color);
        }

        protected void UpdatePointMapperOutputSpace()
        {
            BigDecimal scaleFactor = aspectRatio;
            BigDecimal zoom = Magnification;
            // Predefine minimum and maximum values of the plane, 
            // In order to avoid making unnecisary calculations on each pixel.  

            BigDecimal xMin = -scaleFactor / zoom + Location.Real;
            BigDecimal xMax = scaleFactor / zoom + Location.Real;

            BigDecimal yMin = 2 / zoom + Location.Imag;
            BigDecimal yMax = -2 / zoom + Location.Imag;

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

            AlgorithmInitTask.Wait();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = ThreadCount,
                CancellationToken = TokenSource.Token
            };
            Parallel.For(firstPoint.Y, lastPoint.Y, options, py =>
            {
                if (ShouldSkipRow(py))
                    return;
                var y0 = PointMapper.MapPointY(py);
                Parallel.For(firstPoint.X, lastPoint.X, options, px =>
                {
                    var p = new IntPoint(px, py);
                    if (ShouldSkipPixel(p))
                        return;

                    var x0 = PointMapper.MapPointX(px);

                    PointData pointData = AlgorithmProvider.Run(x0, y0);

                    if (pointData.Escaped)
                    {
                        WritePixelToFrame(p, InnerColor);
                    }
                    else
                    {
                        double colorIndex = PointColorer.GetIndexFromPointData(pointData);
                        WritePixelToFrame(p, OuterColors[colorIndex]);
                    }
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
