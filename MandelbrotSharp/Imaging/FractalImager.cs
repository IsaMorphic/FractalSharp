using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Imaging
{
    public abstract class FractalImager
    {
        public void CreateImage(PointData[,] data, PointColorer colorer, Gradient outerColors, RgbaValue innerColor)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            InitializeImage(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    PointData p = data[y, x];
                    if (p.Escaped)
                    {
                        double index = colorer.GetIndexFromPointData(p);
                        WritePixel(x, y, outerColors[index]);
                    }
                    else
                        WritePixel(x, y, innerColor);
                }
            }
        }
        public abstract void InitializeImage(int width, int height);
        public abstract void WritePixel(int x, int y, RgbaValue color);
    }
}
