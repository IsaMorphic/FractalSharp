using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Imaging
{
    public abstract class ImageProcessor
    {
        public void CreateImage(double[,] indicies, Gradient outerColors, RgbaValue innerColor)
        {
            int width = indicies.GetLength(1);
            int height = indicies.GetLength(0);
            InitializeImage(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double index = indicies[y, x];
                    if (double.IsNaN(index))
                        WritePixel(x, y, innerColor);
                    else
                        WritePixel(x, y, outerColors[index]);
                }
            }
        }
        public abstract void InitializeImage(int width, int height);
        public abstract void WritePixel(int x, int y, RgbaValue color);
    }
}
