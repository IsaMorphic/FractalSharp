using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Quadruple;
namespace MandelBrot.Utilities
{
    class Utils
    {
        public static RGB[] LoadPallete(string path)
        {
            List<RGB> pallete = new List<RGB>();
            StreamReader palleteData = new StreamReader(path);
            while (!palleteData.EndOfStream)
            {
                try
                {
                    string palleteString = palleteData.ReadLine();
                    string[] palleteTokens = palleteString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int r = int.Parse(palleteTokens[0]);
                    int g = int.Parse(palleteTokens[1]);
                    int b = int.Parse(palleteTokens[2]);
                    RGB color = new RGB(r, g, b);
                    pallete.Add(color);
                }
                catch (FormatException) { }
            }
            return pallete.ToArray();
        }
        public static byte[] BitmapToByteArray(Bitmap img)
        {
            // and buffer of appropriate size for storing its bits
            var buffer = new byte[img.Width * img.Height * 4];

            // Now copy bits from bitmap to buffer
            var bits = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
            img.UnlockBits(bits);
            return buffer;
        }

        public static Quad MapQuad(Quad OldValue, Quad OldMin, Quad OldMax, Quad NewMin, Quad NewMax)
        {
            Quad OldRange = (OldMax - OldMin);
            Quad NewRange = (NewMax - NewMin);
            Quad NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }

        public static double Map(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            double OldRange = (OldMax - OldMin);
            double NewRange = (NewMax - NewMin);
            double NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
        public static double lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }
    }
}
