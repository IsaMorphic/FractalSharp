using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Extras
{
    public class SuccessiveRenderSettings : TiledRenderSettings
    {
        private int[] _maxChunkSizes = new int[] { 1 };

        public virtual int[] MaxChunkSizes { get => _maxChunkSizes; set => _maxChunkSizes = value; }
    }
}
