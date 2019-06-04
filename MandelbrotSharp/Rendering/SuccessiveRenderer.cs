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
using MandelbrotSharp.Imaging;

namespace MandelbrotSharp.Rendering
{
    public class SuccessiveRenderer : TiledRenderer
    {
        private int[] ChunkSizes;
        private int[] MaxChunkSizes;

        private int MaxChunkSize => MaxChunkSizes[CellX + CellY * TotalCellsX];
        private int ChunkSize => ChunkSizes[CellX + CellY * TotalCellsX];

        protected void ResetChunkSizes()
        {
            ChunkSizes = (int[])MaxChunkSizes?.Clone();
        }

        protected override bool ShouldSkipRow(int py)
        {
            return (py % ChunkSize != 0);
        }

        protected override bool ShouldSkipPixel(Pixel p)
        {
            var px = p.X;
            var py = p.Y;
            return (px % ChunkSize != 0 || py % ChunkSize != 0) || ((px / ChunkSize) % 2 == 0 && (py / ChunkSize) % 2 == 0 && MaxChunkSize != ChunkSize);
        }

        protected override void WritePixelToFrame(Pixel p, RgbaValue color)
        {
            for (var i = p.X; i < p.X + ChunkSize; i++)
            {
                for (var j = p.Y; j < p.Y + ChunkSize; j++)
                {
                    if (i < Width && j < Height)
                    {
                        CurrentFrame.SetPixel(i, j, color);
                    }
                }
            }
        }

        public void Setup(SuccessiveRenderSettings settings)
        {
            MaxChunkSizes = (int[])settings.MaxChunkSizes.Clone();
            base.Setup(settings);
        }

        protected override void OnConfigurationUpdated()
        {
            ResetChunkSizes();
            base.OnConfigurationUpdated();
        }

        protected override void OnFrameFinished(FrameEventArgs e)
        {
            if (ChunkSize > 1)
                ChunkSizes[CellX + CellY * TotalCellsX] /= 2;
            base.OnFrameFinished(e);
        }
    }
}
