using System;
using System.Threading.Tasks;

namespace FractalSharp.Imaging
{
    public abstract class UpscalingImageBuilder : ImageBuilder
    {
        public void CreateImage(double[,] outerIndicies, double[,] innerIndicies, Gradient outerColors, Gradient innerColors, int newWidth, int newHeight)
        {
            int oldWidth = Math.Min(outerIndicies.GetLength(1), innerIndicies.GetLength(1));
            int oldHeight = Math.Min(outerIndicies.GetLength(0), innerIndicies.GetLength(0));

            int scaleX = newWidth / oldWidth;
            int scaleY = newHeight / oldHeight;

            InitializeImage(newWidth, newHeight);
            Parallel.For(0, newHeight, y =>
            {
                int scaledY = Math.Min(y / scaleY, oldHeight - 1);
                Parallel.For(0, newWidth, x =>
                {
                    int scaledX = Math.Min(x / scaleX, oldWidth - 1);
                    if (double.IsNaN(outerIndicies[scaledY, scaledX]))
                        WritePixel(x, y, innerColors[innerIndicies[scaledY, scaledX]]);
                    else
                        WritePixel(x, y, outerColors[outerIndicies[scaledY, scaledX]]);
                });
            });
        }
    }
}
