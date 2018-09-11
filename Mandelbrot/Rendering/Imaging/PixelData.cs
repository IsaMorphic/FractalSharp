using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Rendering.Imaging
{
    struct PixelData<T>
    {
        private T ZnMagn;
        private int IterCount;

        public T GetZnMagn()
        {
            return ZnMagn;
        }

        public int GetIterCount()
        {
            return IterCount;
        }

        public PixelData(T ZnMagn, int IterCount)
        {
            this.ZnMagn = ZnMagn;
            this.IterCount = IterCount;
        }
    }
}
