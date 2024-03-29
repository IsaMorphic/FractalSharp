/*
 *  Copyright 2018-2024 Chosen Few Software
 *  This file is part of FractalSharp.
 *
 *  FractalSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  FractalSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with FractalSharp.  If not, see <https://www.gnu.org/licenses/>.
 */

using FractalSharp.Algorithms;
using FractalSharp.Algorithms.Coloring;
using FractalSharp.Algorithms.Fractals;
using FractalSharp.Imaging;
using FractalSharp.Numerics.Generic;
using FractalSharp.Numerics.Helpers;
using FractalSharp.Processing;
using QuadrupleLib;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FractalSharp.ExampleApp
{
    unsafe class SkiaImageBuilder : UpscalingImageBuilder
    {
        public SKBitmap Bitmap { get; private set; }

        private byte* skPixels;

        public override void InitializeImage(int width, int height)
        {
            Bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            skPixels = (byte*)Bitmap.GetPixels().ToPointer();
        }

        public override void WritePixel(int x, int y, RgbaValue color)
        {
            var ptr = skPixels + Bitmap.RowBytes * y + x * 4;
            *ptr++ = color.Red;
            *ptr++ = color.Green;
            *ptr++ = color.Blue;
            *ptr++ = color.Alpha;
        }
    }

    class Program
    {
        private const int WIDTH = 2560 * 4;
        private const int HEIGHT = 1440 * 4;

        private static readonly FractalProcessor<SquareMandelbrotAlgorithm<Float128, DefaultNumberConverter>, EscapeTimeParams<Float128>, Float128> FractalProcessor =
            new GPUFractalProcessor<SquareMandelbrotAlgorithm<Float128, DefaultNumberConverter>, Float128>(WIDTH, HEIGHT);

        private static readonly ColorProcessor<SmoothColoringAlgorithm, EmptyColoringParams> OuterColorProcessor =
            new ColorProcessor<SmoothColoringAlgorithm, EmptyColoringParams>(WIDTH, HEIGHT);

        private static readonly ColorProcessor<SingleColorAlgorithm, EmptyColoringParams> InnerColorProcessor =
            new ColorProcessor<SingleColorAlgorithm, EmptyColoringParams>(WIDTH, HEIGHT);

        private static readonly SkiaImageBuilder Imager = new SkiaImageBuilder();

        private static readonly Gradient Colors =
            new Gradient(256, new List<GradientKey>
            {
                new GradientKey(new RgbaValue(0, 0, 0)),
                new GradientKey(new RgbaValue(9, 1, 47)),
                new GradientKey(new RgbaValue(4, 4, 73)),
                new GradientKey(new RgbaValue(0, 7, 100)),
                new GradientKey(new RgbaValue(12, 44, 138)),
                new GradientKey(new RgbaValue(24, 82, 177)),
                new GradientKey(new RgbaValue(57, 125, 209)),
                new GradientKey(new RgbaValue(134, 181, 229)),
                new GradientKey(new RgbaValue(211, 236, 248)),
                new GradientKey(new RgbaValue(241, 233, 191)),
                new GradientKey(new RgbaValue(248, 201, 95)),
                new GradientKey(new RgbaValue(255, 170, 0)),
                new GradientKey(new RgbaValue(204, 128, 0)),
                new GradientKey(new RgbaValue(153, 87, 0)),
                new GradientKey(new RgbaValue(106, 52, 3)),
                new GradientKey(new RgbaValue(66, 30, 15)),
                new GradientKey(new RgbaValue(25, 7, 26))
            });

        static async Task Main(string[] args)
        {
            Console.WriteLine("Process started.");

            int i = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.png").Count();
            while (i < 4500)
            {
                Console.WriteLine($"Computing raw fractal data for frame #{i}...");
                await FractalProcessor.SetupAsync(new ProcessorConfig<EscapeTimeParams<Float128>>
                {
                    ThreadCount = Environment.ProcessorCount,

                    Params = new EscapeTimeParams<Float128>
                    {
                        MaxIterations = 256 * (int)Math.Pow(2, i / 360),
                        Position = new Complex<Float128>(Float128.Parse("-0.743643887037158704752191506114774"), Float128.Parse("0.131825904205311970493132056385139")),
                        Scale = Math.Pow(2, i / 180.0),
                    },
                }, CancellationToken.None);
                PointData<double>[,] inputData = await FractalProcessor.ProcessAsync(CancellationToken.None);

                Console.WriteLine("Computing colors for inner points...");
                await InnerColorProcessor.SetupAsync(new ColorProcessorConfig<EmptyColoringParams>
                {
                    ThreadCount = Environment.ProcessorCount,

                    Params = new EmptyColoringParams(),
                    PointClass = PointClass.Inner,

                    InputData = inputData
                }, CancellationToken.None);
                double[,] innerIndicies = await InnerColorProcessor.ProcessAsync(CancellationToken.None);

                Console.WriteLine("Computing colors for outer points...");
                await OuterColorProcessor.SetupAsync(new ColorProcessorConfig<EmptyColoringParams>
                {
                    ThreadCount = Environment.ProcessorCount,

                    Params = new EmptyColoringParams(),
                    PointClass = PointClass.Outer,

                    InputData = inputData
                }, CancellationToken.None);
                double[,] outerIndicies = await OuterColorProcessor.ProcessAsync(CancellationToken.None);

                Console.WriteLine("Building image...");
                Imager.CreateImage(outerIndicies, innerIndicies, Colors, Colors);

                Console.WriteLine("Writing image file to disk...");
                Imager.Bitmap.Encode(new SKFileWStream($"{i:D4}.png"), SKEncodedImageFormat.Png, 100);

                Console.WriteLine("Image rendered successfully!");
                i++;
            }
        }
    }
}