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

    public class PointMapper<TNumberIn, TNumberOut> where TNumberIn : struct where TNumberOut : struct
    {
        public Rectangle<TNumberIn> InputSpace { get; set; }
        public Rectangle<TNumberOut> OutputSpace { get; set; }

        public Number<TNumberOut> MapPointX(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(),
                InputSpace.XMin.As<TNumberOut>(), InputSpace.XMax.As<TNumberOut>(),
                OutputSpace.XMin, OutputSpace.XMax);
        }

        public Number<TNumberOut> MapPointY(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(),
                InputSpace.YMin.As<TNumberOut>(), InputSpace.YMax.As<TNumberOut>(),
                OutputSpace.YMin, OutputSpace.YMax);
        }

        private static Number<TNumber> MapValue<TNumber>(Number<TNumber> OldValue, Number<TNumber> OldMin, Number<TNumber> OldMax, Number<TNumber> NewMin, Number<TNumber> NewMax) where TNumber : struct
        {
            Number<TNumber> OldRange = OldMax - OldMin;
            Number<TNumber> NewRange = NewMax - NewMin;
            Number<TNumber> NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
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

        protected PointMapper<int, TNumber> PointMapper { get; private set; }

        protected TAlgorithm AlgorithmProvider { get; private set; }
        protected PointColorer PointColorer { get; private set; }

        protected RenderSettings Settings { get; private set; }

        private CancellationTokenSource TokenSource { get; set; }
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

            PointMapper = new PointMapper<int, TNumber>();
            PointMapper.InputSpace = new Rectangle<int>(0, Width, 0, Height);
        }

        public void Setup(RenderSettings settings)
        {
            Settings = settings.Copy();

            AlgorithmProvider = new TAlgorithm();
            PointColorer = new PointColorer();
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

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        private void RenderFrame()
        {
            // Fire frame start event
            OnFrameStarted();

            if (!AlgorithmProvider.Initialized)
            {
                AlgorithmProvider.Initialize(Settings.Params.Copy(), TokenSource.Token);

                Number<TNumber> aspectRatio = Number<TNumber>.From(Width) / Number<TNumber>.From(Height);
                PointMapper.OutputSpace = AlgorithmProvider.GetOutputBounds(aspectRatio);
            }

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
