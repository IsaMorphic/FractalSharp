using MandelbrotSharp.Utilities;

namespace MandelbrotSharp.Imaging
{
    public struct RgbaValue
    {
        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        public RgbaValue(byte r, byte g, byte b, byte a = 255)
        {
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }

        public static explicit operator int(RgbaValue rgba) {
            return (rgba.alpha << 24) | (rgba.red << 16) | (rgba.green << 8) | (rgba.blue);
        }

        public static RgbaValue LerpColors(RgbaValue a, RgbaValue b, double alpha) {
            // Initialize final color
            RgbaValue c = new RgbaValue();

            // Linear interpolate red, green, and blue values.
            c.red = (byte)Utils.lerp(a.red, b.red, alpha);

            c.green = (byte)Utils.lerp(a.green, b.green, alpha);

            c.blue = (byte)Utils.lerp(a.blue, b.blue, alpha);

            c.alpha = (byte)Utils.lerp(a.alpha, b.alpha, alpha);

            return c;
        }
    }
}
