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
using MandelbrotSharp.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Processing
{
    public interface IFractalProcessor
    {
        int Width { get; }
        int Height { get; }

        void Setup(ProcessorConfig settings);
        Task<PointData[,]> RenderFrameAsync(CancellationToken cancellationToken);
    }

    public abstract class FractalProcessor<TNumber, TAlgorithm> : IFractalProcessor
            where TAlgorithm : IAlgorithmProvider<TNumber>, new()
            where TNumber : struct
    {

        public int Width { get; private set; }
        public int Height { get; private set; }

        protected PointMapper<int, TNumber> PointMapper { get; private set; }

        protected TAlgorithm AlgorithmProvider { get; private set; }

        protected ProcessorConfig Settings { get; private set; }

        #region Initialization and Configuration Methods

        public FractalProcessor(int width, int height)
        {
            Width = width;
            Height = height;

            PointMapper = new PointMapper<int, TNumber>();
            PointMapper.InputSpace = new Rectangle<int>(0, Width, 0, Height);
        }

        public void Setup(ProcessorConfig settings)
        {
            Settings = settings.Copy();
            AlgorithmProvider = new TAlgorithm();
        }
        #endregion

        #region Rendering Methods

        public Task<PointData[,]> RenderFrameAsync(CancellationToken cancellationToken)
        {
            return RenderFrame(cancellationToken);
        }

        // Frame rendering method, using generic typing to reduce the amount 
        // of code used and to make the algorithm easily applicable to other number types
        private async Task<PointData[,]> RenderFrame(CancellationToken cancellationToken)
        {
            if (!AlgorithmProvider.Initialized)
            {
                await AlgorithmProvider.Initialize(Settings.Params.Copy(), cancellationToken);

                Number<TNumber> aspectRatio = Number<TNumber>.From(Width) / Number<TNumber>.From(Height);
                PointMapper.OutputSpace = AlgorithmProvider.GetOutputBounds(aspectRatio);
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Settings.ThreadCount,
                CancellationToken = cancellationToken
            };

            return RenderFrame(options);
        }

        protected abstract PointData[,] RenderFrame(ParallelOptions options);

        #endregion
    }
}
