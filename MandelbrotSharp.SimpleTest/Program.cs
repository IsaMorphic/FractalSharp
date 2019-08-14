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
        static void Main(string[] args)
        {
            DefaultRenderer<double, MandelbrotNAlgorithm<double>> renderer =
                new DefaultRenderer<double, MandelbrotNAlgorithm<double>>(8192, 8192);

            RgbaValue[] colors = new RgbaValue[]
            {
                new RgbaValue(0, 0, 255),
                new RgbaValue(255, 0, 0)
            };

            renderer.Setup(new RenderSettings<double>
            {
                InnerColor = new RgbaValue(0, 0, 0),
                OuterColors = new Gradient(colors, 256),
                ThreadCount = Environment.ProcessorCount,
                Params = new MandelbrotNParams<double>
                {
                    MaxIterations = 256,
                    EscapeRadius = 4.0,
                    Magnification = 1.0,
                    Location = new Complex<double>(0.0, 0.0),
                }
            });

            renderer.FrameFinished += Renderer_FrameFinished;
            renderer.StartRenderFrame().Wait();
        }

        private static unsafe void Renderer_FrameFinished(object sender, FrameEventArgs e)
        {
            SKBitmap bitmap = new SKBitmap(e.Frame.Width, e.Frame.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            int[] bits = e.Frame.CopyDataAsBits();
            bitmap.SetPixels((IntPtr)bits.AsMemory().Pin().Pointer);
            SKFileWStream stream = new SKFileWStream("result.png");
            SKPixmap.Encode(stream, bitmap, SKEncodedImageFormat.Png, 100);
        }
    }
}