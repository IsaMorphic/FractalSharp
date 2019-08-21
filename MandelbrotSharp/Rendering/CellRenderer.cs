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
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public class CellRenderer<TNumber, TAlgorithm> : FractalRenderer<TNumber, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<TNumber>, new()
        where TNumber : struct
    {
        public CellRenderer(int width, int height) : base(width, height)
        {
        }

        protected int CellX { get; private set; }
        protected int CellY { get; private set; }

        protected int CellWidth => Width / Settings.CellsX;
        protected int CellHeight => Height / Settings.CellsY;

        protected new CellRenderSettings Settings { get; private set; }

        public void Setup(CellRenderSettings settings)
        {
            Settings = settings;
            base.Setup(Settings);
        }

        protected virtual void UpdateCellCoords()
        {
            if (CellX < Settings.CellsX - 1) { CellX++; }
            else if (CellY < Settings.CellsY - 1) { CellX = 0; CellY++; }
            else { CellX = 0; CellY = 0; }
        }

        protected override void OnFrameFinished(FrameEventArgs e)
        {
            UpdateCellCoords();
            base.OnFrameFinished(e);
        }

        protected override void RenderFrame(ParallelOptions options)
        {
            Parallel.For(CellY * CellHeight, (CellY + 1) * CellHeight, options, py =>
            {
                var y0 = PointMapper.MapPointY(py);

                Parallel.For(CellX * CellWidth, (CellX + 1) * CellWidth, options, px =>
                {
                    var x0 = PointMapper.MapPointX(px);

                    PointData pointData = AlgorithmProvider.Run(new Complex<TNumber>(x0, y0));

                    if (pointData.Escaped)
                    {
                        double colorIndex = PointColorer.GetIndexFromPointData(pointData);
                        CurrentFrame.SetPixel(px, py, Settings.OuterColors[colorIndex]);
                    }
                    else
                    {
                        CurrentFrame.SetPixel(px, py, Settings.InnerColor);
                    }
                });
            });
        }
    }
}
