using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Imaging
{
    public class PixelData
    {
        double Magnitude;
        private int IterCount;
        private bool Escaped;

        public double GetMagnitude()
        {
            return Magnitude;
        }

        public int GetIterCount()
        {
            return IterCount;
        }

        public bool GetEscaped()
        {
            return Escaped;
        }

        public PixelData(double Magnitude, int IterCount, bool Escaped)
        {
            this.Magnitude = Magnitude;
            this.IterCount = IterCount;
            this.Escaped = Escaped;
        }
    }
}
