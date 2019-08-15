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
using MandelbrotSharp.Data;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using System;
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

        public RgbaImage Frame { get; }
    }

    public abstract class FractalRenderer<TNumber, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<TNumber>, new()
        where TNumber : struct 
    {
        public event EventHandler FrameStarted;
        public event EventHandler RenderHalted;
        public event EventHandler<FrameEventArgs> FrameFinished;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TaskStatus? RenderStatus => RenderTask?.Status;

        protected RgbaImage CurrentFrame { get; private set; }

        protected TAlgorithm AlgorithmProvider { get; private set; }
        protected PointMapper<int, TNumber> PointMapper { get; private set; }
        protected PointColorer PointColorer { get; private set; }

        protected RenderSettings Settings { get; private set; }

        private CancellationTokenSource TokenSource { get; set; }
        private Task AlgorithmInitTask { get; set; }
        private Task RenderTask { get; set; }

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

        public FractalRenderer(int width, int height)
        {
            Width = width;
            Height = height;

            CurrentFrame = new RgbaImage(Width, Height);
        }

        public void Setup(RenderSettings settings)
        {
            Settings = settings;

            AlgorithmProvider = new TAlgorithm();
            PointColorer = new PointColorer();
            InitPointMapper();
        }

        private void InitPointMapper()
        {

            Number<TNumber> aspectRatio = Number<TNumber>.From(Width) / Height;

            Number<TNumber> xMax = aspectRatio * 2 / 
                Settings.Params.Magnification.As<TNumber>() + 
                Settings.Params.Location.Real.As<TNumber>();
            Number<TNumber> xMin = -xMax;

            Number<TNumber> yMax = 
                2 / Settings.Params.Magnification.As<TNumber>() + 
                    Settings.Params.Location.Imag.As<TNumber>();
            Number<TNumber> yMin = -yMax;

            PointMapper = new PointMapper<int, TNumber>(
                new Rectangle<int>(0, Width, 0, Height),
                new Rectangle<TNumber>(xMin, xMax, yMin, yMax)
            );
        }
        #endregion

        #region Rendering Methods

        public Task StartRenderFrame()
        {
            if (RenderStatus == TaskStatus.Running)
                throw new Exception("The running task has not yet completed.");

            TokenSource = new CancellationTokenSource();
            RenderTask = Task.Factory.StartNew(RenderFrame, TokenSource.Token);
            return RenderTask.ContinueWith(RenderTaskFinished);
        }

        private void InitAlgorithmProvider()
        {
            AlgorithmInitTask = Task.Run(() => AlgorithmProvider
            .Initialize(Settings.Params, TokenSource.Token));
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        private void RenderFrame()
        {
            // Fire frame start event
            OnFrameStarted();

            // Initialize the algorithm provider if it wasn't initialized already
            if (AlgorithmInitTask == null)
                InitAlgorithmProvider();

            // Wait for algorithm provider to initialize
            AlgorithmInitTask.Wait();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Settings.ThreadCount,
                CancellationToken = TokenSource.Token
            };

            RenderFrame(options);
        }

        protected abstract void RenderFrame(ParallelOptions options);

        private void RenderTaskFinished(Task task)
        {
            if (task != RenderTask)
                throw new Exception("This is not the task you are looking for...");

            RenderTask.Dispose();
            TokenSource.Dispose();

            if (RenderStatus == TaskStatus.Canceled)
            {
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
