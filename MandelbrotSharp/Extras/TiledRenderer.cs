using MandelbrotSharp.Mathematics;
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Extras
{
    public class TiledRenderer : MandelbrotRenderer
    {
        protected int TotalCellsX { get; private set; } = 1;
        protected int TotalCellsY { get; private set; } = 1;

        protected int CellX { get; private set; }
        protected int CellY { get; private set; }

        protected int CellWidth => Width / TotalCellsX;
        protected int CellHeight => Height / TotalCellsY;

        protected void IncrementCellCoords()
        {
            if (CellX < TotalCellsX - 1) { CellX++; }
            else if (CellY < TotalCellsY - 1) { CellX = 0; CellY++; }
            else { CellX = 0; CellY = 0; }
        }

        protected override void OnFrameFinished(FrameEventArgs e)
        {
            IncrementCellCoords();
            base.OnFrameFinished(e);
        }

        protected override Pixel GetFrameFirstPixel()
        {
            return new Pixel(CellX * CellWidth, CellY * CellHeight);
        }

        protected override Pixel GetFrameLastPixel()
        {
            return new Pixel((CellX + 1) * CellWidth, (CellY + 1) * CellHeight);
        }

        protected override void OnConfigurationUpdated(ConfigEventArgs e)
        {
            var settings = e.Settings as TiledRenderSettings;
            if (settings != null)
            {
                TotalCellsX = settings.TilesX;
                TotalCellsY = settings.TilesY;
            }
            base.OnConfigurationUpdated(e);
        }
    }
}
