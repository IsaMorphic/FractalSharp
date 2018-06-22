using Quadruple;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelBrot.Utilities
{
    class FractalSettings
    {
        public int frameCount = 0;
        public int max_iteration = 100;
        public decimal offsetX = -0.743643887037158704752191506114774M;
        public decimal offsetY = 0.131825904205311970493132056385139M;
        public Size fractalSize = new Size(640, 480);
        public string palettePath;
        public string videoPath;
        public bool extraPrecision;
        public int version = 0;
    }
}
