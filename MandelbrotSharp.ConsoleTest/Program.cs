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
using System;
using SkiaSharp;
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MandelbrotSharp.Rendering;
using System.Buffers;

namespace MandelbrotSharp.ConsoleTest
{
    class Program
    {
        private static DefaultRenderer<double, SquareMandelbrotAlgorithm<double>> Renderer =
            new DefaultRenderer<double, SquareMandelbrotAlgorithm<double>>(1024, 1024);
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

        static void Main(string[] args)
        {
            Renderer.Setup(new RenderSettings
            {
                OuterColors = Colors,
                InnerColor = new RgbaValue(0, 0, 0),

                ThreadCount = Environment.ProcessorCount,

                Params = new SquareMandelbrotParams<double>
                {
                    MaxIterations = 256,
                    Magnification = 1.0,
                    Location = Complex<double>.Zero,
                    EscapeRadius = 4.0,
                }
            });

            Renderer.FrameFinished += FrameFinished;
            Console.WriteLine("Rendering image, please wait...");
            Renderer.StartRenderFrame().Wait();
            Console.WriteLine("Image rendered successfully!");
        }

        private static unsafe void FrameFinished(object sender, FrameEventArgs e)
        {
            SKBitmap bitmap = new SKBitmap(e.Frame.Width, e.Frame.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            MemoryHandle pixels = e.Frame.CopyDataAsBits().AsMemory().Pin();
            bitmap.SetPixels((IntPtr)pixels.Pointer);
            SKFileWStream stream = new SKFileWStream("output.png");
            SKPixmap.Encode(stream, bitmap, SKEncodedImageFormat.Png, 100);
            stream.Dispose();
            bitmap.Dispose();
            pixels.Dispose();
        }
    }
}