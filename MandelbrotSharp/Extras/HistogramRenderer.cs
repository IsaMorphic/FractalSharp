using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Extras
{
    public class HistogramRenderer : MandelbrotRenderer
    {
        private RgbaValue[] palette;

        public void SetPalette(RgbaValue[] newPalette) {
            palette = newPalette;
        }

        protected override RgbaValue GetColorFromPixelData(PixelData data)
        {
            if (data.Escaped)
                return new RgbaValue(0, 0, 0);
            
            double temp_i = data.IterCount;
            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(data.Magnitude) / 2;
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            // Rearranging the potential function.
            // Dividing log_zn by log(2) instead of log(N = 1<<8)
            // because we want the entire palette to range from the
            // center to radius 2, NOT our bailout radius.
            temp_i = temp_i + 1 - nu;
            // Grab two colors from the pallete
            RgbaValue color1 = palette[(int)temp_i % (palette.Length - 1)];
            RgbaValue color2 = palette[(int)(temp_i + 1) % (palette.Length - 1)];

            // Lerp between both colors
            RgbaValue final = RgbaValue.LerpColors(color1, color2, temp_i % 1);

            // Return the result.
            return final;
        }
    }
}
