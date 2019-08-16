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
using System.Linq;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public class SuccessiveRenderer<TNumber, TAlgorithm> : TiledRenderer<TNumber, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<TNumber>, new()
        where TNumber : struct
    {
        private int[] ChunkSizes { get; set; }

        private int MaxChunkSize => Settings.MaxChunkSizes[CellX + CellY * Settings.TilesX];
        private int ChunkSize => ChunkSizes[CellX + CellY * Settings.TilesY];

        protected new SuccessiveRenderSettings Settings { get; private set; }

        public bool RenderedToCompletion => ChunkSizes.All(n => n == 1);

        public SuccessiveRenderer(int width, int height) : base(width, height)
        {
        }

        public void Setup(SuccessiveRenderSettings settings)
        {
            Settings = settings;
            ResetChunkSizes();
            base.Setup(settings);
        }

        protected void ResetChunkSizes()
        {
            ChunkSizes = new int[Settings.MaxChunkSizes.Length];
            Array.Copy(Settings.MaxChunkSizes, ChunkSizes, ChunkSizes.Length);
        }

        protected override void RenderFrame(ParallelOptions options)
        {
            Parallel.For(CellY * CellHeight, (CellY + 1) * CellHeight, options, py =>
            {
                if (py % ChunkSize != 0)
                    return;

                var y0 = PointMapper.MapPointY(py);

                Parallel.For(CellX * CellWidth, (CellX + 1) * CellWidth, options, px =>
                {
                    if ((px % ChunkSize != 0 || py % ChunkSize != 0) || ((px / ChunkSize) % 2 == 0 && (py / ChunkSize) % 2 == 0 && MaxChunkSize != ChunkSize))
                        return;

                    var x0 = PointMapper.MapPointX(px);

                    PointData pointData = AlgorithmProvider.Run(new Complex<TNumber>(x0, y0));

                    if (pointData.Escaped)
                    {
                        double colorIndex = PointColorer.GetIndexFromPointData(pointData);
                        WriteChunkToFrame(px, py, Settings.OuterColors[colorIndex]);
                    }
                    else
                    {
                        WriteChunkToFrame(px, py, Settings.InnerColor);
                    }
                });
            });
        }

        protected override void OnFrameFinished(FrameEventArgs e)
        {
            if (!RenderedToCompletion)
                ChunkSizes[CellX + CellY * Settings.TilesX] /= 2;

            base.OnFrameFinished(e);
        }

        private void WriteChunkToFrame(int px, int py, RgbaValue color)
        {
            for (var i = px; i < px + ChunkSize; i++)
                for (var j = py; j < py + ChunkSize; j++)
                    if (i < Width && j < Height)
                        CurrentFrame.SetPixel(i, j, color);
        }
    }
}
