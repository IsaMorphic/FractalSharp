using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MandelbrotSharp.Utilities;

namespace MandelbrotSharp.Imaging
{
    public struct RgbValue
    {
        public int red;
        public int green;
        public int blue;
        public RgbValue(int r, int g, int b)
        {
            red = r;
            green = g;
            blue = b;
        }

        public static RgbValue LerpColors(RgbValue a, RgbValue b, double alpha) {
            // Initialize final color
            RgbValue c = new RgbValue();

            // Linear interpolate red, green, and blue values.
            c.red = (int)Utils.lerp(a.red, b.red, alpha);

            c.green = (int)Utils.lerp(a.green, b.green, alpha);

            c.blue = (int)Utils.lerp(a.blue, b.blue, alpha);

            return c;
        }

        public Color toColor() {
            return Color.FromArgb(red, green, blue);
        }
    }
}
