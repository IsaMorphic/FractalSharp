using System;
using SkiaSharp;
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MandelbrotSharp.Rendering;

namespace MandelbrotSharp.SimpleTest
{
    class Program
    {
        static DefaultRenderer<double, MandelbrotNAlgorithm<double>> Renderer =
            new DefaultRenderer<double, MandelbrotNAlgorithm<double>>(32768, 32768);
        static RgbaValue[] Colors = new RgbaValue[]
        {
            new RgbaValue(0, 0, 0),
            new RgbaValue(0, 0, 255),
            new RgbaValue(0, 255, 0),
            new RgbaValue(0, 0, 255)
        };

        static void Main(string[] args)
        {
            Renderer.Setup(new RenderSettings
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
            bitmap.SetPixels((IntPtr)e.Frame.CopyDataAsBits().AsMemory().Pin().Pointer);
            SKFileWStream stream = new SKFileWStream("result.png");
            SKPixmap.Encode(stream, bitmap, SKEncodedImageFormat.Png, 100);
            stream.Dispose();
        }
    }
}