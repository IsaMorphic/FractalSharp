using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Imaging
{
    public class PixelData
    {
        public double Magnitude { get; private set; }
        public int IterCount { get; private set; }
        public bool Escaped { get; private set; }

        public PixelData(double Magnitude, int IterCount, bool Escaped)
        {
            this.Magnitude = Magnitude;
            this.IterCount = IterCount;
            this.Escaped = Escaped;
        }
    }
}
