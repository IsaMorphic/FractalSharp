using System;
using System.Collections.Generic;
using System.Text;

namespace FractalSharp.Imaging
{
    public class GradientKey
    {
        public RgbaValue Color { get; set; }

        public GradientKey(RgbaValue color)
        {
            Color = color;
        }
    }
}
