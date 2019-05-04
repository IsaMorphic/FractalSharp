using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Imaging
{
    public class PixelData
    {
        public Complex ZValue { get; private set; }
        public int IterCount { get; private set; }
        public bool Escaped { get; private set; }

        public PixelData(Complex ZValue, int IterCount, bool Escaped)
        {
            this.ZValue = ZValue;
            this.IterCount = IterCount;
            this.Escaped = Escaped;
        }
    }
}
