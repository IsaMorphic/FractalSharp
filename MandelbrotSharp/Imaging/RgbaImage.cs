using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Imaging
{
    public class RgbaImage
    {
        public RgbaValue[] data;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public RgbaImage(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            data = new RgbaValue[Width*Height];
        }

        public RgbaImage(RgbaImage image) {
            Width = image.Width;
            Height = image.Height;
            data = new RgbaValue[Width * Height];
            Array.Copy(image.data, data, image.data.Length);
        }

        public void SetPixel(int x, int y, RgbaValue argb)
        {
            data[x + y * Width] = argb;
        }

        public RgbaValue GetPixel(int x, int y)
        {
            return data[x + Width * y];
        }

        public int[] CopyDataAsBits()
        {
            return Array.ConvertAll(data, argb => (int)argb);
        }
    }
}
