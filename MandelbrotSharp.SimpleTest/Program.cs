using System;
using SkiaSharp;
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MandelbrotSharp.Rendering;
using System.Linq;

namespace MandelbrotSharp.SimpleTest
{
    class Program
    {
        static int Count = 0;
        static SuccessiveRenderer<double, MandelbrotNAlgorithm<double>> Renderer =
            new SuccessiveRenderer<double, MandelbrotNAlgorithm<double>>(4096, 4096);
        static RgbaValue[] Colors = new RgbaValue[]
        {
            new RgbaValue(0, 0, 255),
            new RgbaValue(255, 0, 0)
        };
        static void Main(string[] args)
        {
            Renderer.Setup(new SuccessiveRenderSettings<double>
            {
                InnerColor = new RgbaValue(0, 0, 0),
                OuterColors = new Gradient(Colors, 256),
                ThreadCount = Environment.ProcessorCount,
                Params = new MandelbrotNParams<double>
                {
                    MaxIterations = 256,
                    EscapeRadius = 4.0,
                    Magnification = 1.0,
                    Location = new Complex<double>(0.0, 0.0),
                },
                MaxChunkSizes = Enumerable.Repeat(16, 4).ToArray(),
                TilesX = 2,
                TilesY = 2
            });

            Renderer.FrameFinished += FrameFinished;
            Renderer.StartRenderFrame();
            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey();
        }

        private static unsafe void FrameFinished(object sender, FrameEventArgs e)
        {
            SKBitmap bitmap = new SKBitmap(e.Frame.Width, e.Frame.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            int[] bits = e.Frame.CopyDataAsBits();
            bitmap.SetPixels((IntPtr)bits.AsMemory().Pin().Pointer);
            SKFileWStream stream = new SKFileWStream($"result_{Count}.png");
            SKPixmap.Encode(stream, bitmap, SKEncodedImageFormat.Png, 100);

            if (Renderer.RenderedToCompletion)
            {
                Console.WriteLine("Render Completed.");
            }
            else
            {
                Renderer.StartRenderFrame();
                Count++;
            }
        }
    }
}