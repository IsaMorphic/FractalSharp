using MandelbrotSharp.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Extras
{
    public class HistogramRenderSettings : SuccessiveRenderSettings
    {
        private RgbaValue[] _palette;

        public virtual RgbaValue[] Palette { get => _palette; set => _palette = value; }
    }
}
