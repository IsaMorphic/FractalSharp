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

        private static readonly FractalProcessor<SquareMandelbrotAlgorithm<double, DefaultNumberConverter>, EscapeTimeParams<double>, double> FractalProcessor =
            new GPUFractalProcessor<SquareMandelbrotAlgorithm<double, DefaultNumberConverter>, double>(WIDTH, HEIGHT);

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

        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        static int? GetLastFrameIndex(string directoryPath) 
        {
            return Directory.GetFiles(directoryPath, "*.png")
                        .Select<string, int?>(x => int.TryParse(Path.GetFileNameWithoutExtension(x), out int frameNum) ? frameNum + 1 : null)
                        .Where(n => n is not null)
                        .OrderByDescending(n => n)
                        .FirstOrDefault();
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Process started.");

            Console.CancelKeyPress += OnCancelKeyPress;

            int prevLastFrame, currLastFrame = GetLastFrameIndex(Environment.CurrentDirectory) - 8 ?? 0, i;
            bool frameFinished;
            do
            {
                bool frameFound = false;
                do
                {
                    prevLastFrame = currLastFrame;
                    currLastFrame = GetLastFrameIndex(Environment.CurrentDirectory) ?? 0;

                    for (i = prevLastFrame; i <= currLastFrame && currLastFrame > 0;)
                    {
                        try
                        {
                            using var frameStream = File.OpenRead($"{i:D4}.png");
                            if (frameStream.Length == 0)
                            {
                                Console.WriteLine($"Found next frame to do: {i}; locking now.");
                                frameFound = true;
                                break;
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine($"Found next frame to do: {i}; locking now.");
                            frameFound = true;
                            break;
                        }
                        catch (IOException)
                        {
                            Console.WriteLine($"Skipping locked frame: {i}...");
                        }
                        i++;
                    }
                } while (!frameFound);

                // touch file to allocate it (effectively lock the file)
                using SKWStream outputStream = new SKFileWStream($"{i:D4}.png");
                frameFinished = false;

                try
                {
                    Console.WriteLine($"Computing raw fractal data for frame #{i}...");
                    await FractalProcessor.SetupAsync(new ProcessorConfig<EscapeTimeParams<double>>
                    {
                        ThreadCount = Environment.ProcessorCount,

                        Params = new EscapeTimeParams<double>
                        {
                            MaxIterations = 256 * (int)Math.Pow(2, i / 360),
                            Position = new Complex<double>(double.Parse("-0.743643887037158704752191506114774"), double.Parse("0.131825904205311970493132056385139")),
                            Scale = Math.Pow(2, i / 180.0),
                        },
                    }, cts.Token);
                    PointData<double>[,] inputData = await FractalProcessor.ProcessAsync(cts.Token);

                    Console.WriteLine("Computing colors for inner points...");
                    await InnerColorProcessor.SetupAsync(new ColorProcessorConfig<EmptyColoringParams>
                    {
                        ThreadCount = Environment.ProcessorCount,

                        Params = new EmptyColoringParams(),
                        PointClass = PointClass.Inner,

                        InputData = inputData
                    }, cts.Token);
                    double[,] innerIndicies = await InnerColorProcessor.ProcessAsync(cts.Token);

                    Console.WriteLine("Computing colors for outer points...");
                    await OuterColorProcessor.SetupAsync(new ColorProcessorConfig<EmptyColoringParams>
                    {
                        ThreadCount = Environment.ProcessorCount,

                        Params = new EmptyColoringParams(),
                        PointClass = PointClass.Outer,

                        InputData = inputData
                    }, cts.Token);
                    double[,] outerIndicies = await OuterColorProcessor.ProcessAsync(cts.Token);

                    Console.WriteLine("Building image...");
                    Imager.CreateImage(outerIndicies, innerIndicies, Colors, Colors);

                    Console.WriteLine("Writing image file to disk...");
                    Imager.Bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                    Console.WriteLine("Image rendered successfully!");
                    frameFinished = true;
                }
                catch (AggregateException ex) when (ex.InnerExceptions.Any(err => err.GetType() == typeof(OperationCanceledException))) { }
                catch (OperationCanceledException) { }
            } while (!cts.IsCancellationRequested);
            cts.Dispose();

            if (!frameFinished)
            {
                File.Delete($"{i:D4}.png");
            }

            Console.WriteLine("Process halted. All remaining tasks completed gracefully!");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancellation registered! Waiting for current processing to complete...");
            cts.Cancel();
            e.Cancel = true;
        }
    }
}