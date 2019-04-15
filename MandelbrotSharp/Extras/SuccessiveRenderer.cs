using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using MandelbrotSharp.Rendering;

namespace MandelbrotSharp.Extras
{
    public class SuccessiveRenderer : TiledRenderer
    {
        private int[] ChunkSizes;
        private int[] MaxChunkSizes;

        private int MaxChunkSize => MaxChunkSizes[CellX + CellY * TotalCellsX];
        private int ChunkSize => ChunkSizes[CellX + CellY * TotalCellsX];

        protected void ResetChunkSizes()
        {
            ChunkSizes = (int[])MaxChunkSizes.Clone();
        }

        protected override bool ShouldSkipPixel(Pixel p)
        {
            int px = p.X;
            int py = p.Y;
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

        protected override void OnConfigurationUpdated(ConfigEventArgs e)
        {
            var settings = e.Settings as SuccessiveRenderSettings;
            MaxChunkSizes = (int[])settings?.MaxChunkSizes.Clone();
            ResetChunkSizes();
            base.OnConfigurationUpdated(e);
        }

        protected override void OnFrameFinished(FrameEventArgs e)
        {
            if (ChunkSize > 1)
                ChunkSizes[CellX + CellY * TotalCellsX] /= 2;
            base.OnFrameFinished(e);
        }
    }
}
