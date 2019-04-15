using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Extras
{
    public class TiledRenderSettings : RenderSettings
    {
        private int _tilesX = 1;
        private int _tilesY = 1;

        public virtual int TilesY { get => _tilesY; set => _tilesY = value; }
        public virtual int TilesX { get => _tilesX; set => _tilesX = value; }
    }
}
