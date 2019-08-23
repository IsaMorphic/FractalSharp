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
        private static DefaultRenderer<double, MandelbrotAlgorithm<double>> Renderer =
            new DefaultRenderer<double, MandelbrotAlgorithm<double>>(32768, 32768);
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

                Params = new MandelbrotParams<double>
                {
                    MaxIterations = 256,
                    EscapeRadius = 4.0,
                    Magnification = 1.0,
                    Location = new Complex<double>(0.0, 0.0),
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