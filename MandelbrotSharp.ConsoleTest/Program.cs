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
using MandelbrotSharp.Algorithms.Coloring;
using MandelbrotSharp.Algorithms.Fractals;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MandelbrotSharp.Processing;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.ConsoleTest
{
    class SkiaImager : ImageProcessor
    {
        public SKBitmap Bitmap { get; private set; }

        public override void InitializeImage(int width, int height)
        {
            Bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        }

        public override void WritePixel(int x, int y, RgbaValue color)
        {
            Bitmap.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, color.Alpha));
        }
    }

    class Program
    {
        private static readonly FractalProcessor<double, SquareMandelbrotAlgorithm<double>> FractalProcessor =
            new FractalProcessor<double, SquareMandelbrotAlgorithm<double>>(3840, 2160);

        private static readonly ColorProcessor<SmoothColoringAlgorithm> ColorProcessor =
            new ColorProcessor<SmoothColoringAlgorithm>(3840, 2160);

        private static readonly SkiaImager Imager = new SkiaImager();

        private static Gradient Colors = new Gradient(new RgbaValue[]
        {
            new RgbaValue(9, 1, 47),
            new RgbaValue(4, 4, 73),
            new RgbaValue(0, 7, 100),
            new RgbaValue(12, 44, 138),
            new RgbaValue(24, 82, 177),
            new RgbaValue(57, 125, 209),
            new RgbaValue(134, 181, 229),
            new RgbaValue(211, 236, 248),
            new RgbaValue(241, 233, 191),
            new RgbaValue(248, 201, 95),
            new RgbaValue(255, 170, 0),
            new RgbaValue(204, 128, 0),
            new RgbaValue(153, 87, 0),
            new RgbaValue(106, 52, 3),
            new RgbaValue(66, 30, 15),
            new RgbaValue(25, 7, 26),
        }, 256);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Rendering image, please wait...");

            await FractalProcessor.SetupAsync(new ProcessorConfig
            {
                ThreadCount = Environment.ProcessorCount,

                Params = new SquareMandelbrotParams<double>
                {
                    MaxIterations = 256,
                    Magnification = 1.0,
                    Location = Complex<double>.Zero,
                    EscapeRadius = 4.0,
                }
            }, CancellationToken.None);
            PointData[,] inputData = await FractalProcessor.ProcessAsync(CancellationToken.None);

            await ColorProcessor.SetupAsync(new ColorProcessorConfig
            {
                ThreadCount = Environment.ProcessorCount,
                Params = new SmoothColoringParams(),
                InputData = inputData
            }, CancellationToken.None);
            double[,] indicies = await ColorProcessor.ProcessAsync(CancellationToken.None);

            Imager.CreateImage(indicies, Colors, new RgbaValue(0, 0, 0));
            SKPixmap.Encode(new SKFileWStream("output.png"), Imager.Bitmap, SKEncodedImageFormat.Png, 100);

            Console.WriteLine("Image rendered successfully!");
        }
    }
}