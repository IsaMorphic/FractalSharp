using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Imaging
{
    class PixelData<T>
    {
        private T ZnMagn;
        private int IterCount;
        private bool BelowMaxIter;

        public T GetZnMagn()
        {
            return ZnMagn;
        }

        public int GetIterCount()
        {
            return IterCount;
        }

        public bool Escaped()
        {
            return BelowMaxIter;
        }

        public PixelData(T ZnMagn, int IterCount, bool BelowMaxIter)
        {
            this.ZnMagn = ZnMagn;
            this.IterCount = IterCount;
            this.BelowMaxIter = BelowMaxIter;
        }
    }
}
