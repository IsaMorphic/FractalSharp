using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MandelbrotSharp.Imaging
{
    public class PixelColorator
    {
        public virtual double GetPaletteIndexFromPixelData(PixelData data)
        {
            if (data.Escaped)
                return 0;

            // sqrt of inner term removed using log simplification rules.
            double log_zn = Math.Log(data.ZValue.Magnitude);
            double nu = Math.Log(log_zn / Math.Log(2)) / Math.Log(2);
            // Rearranging the potential function.
            // Dividing log_zn by log(2) instead of log(N = 1<<8)
            // because we want the entire palette to range from the
            // center to radius 2, NOT our bailout radius.

            // Return the result.
            return data.IterCount + 1 - nu;
        }
    }
}
