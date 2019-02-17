using MandelbrotSharp.Imaging;
using MandelbrotSharp.Mathematics;
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public abstract class AlgorithmProvider<T>
    {
        protected readonly GenericMath<T> TMath;
        protected AlgorithmParams Params { get; private set; }

        public AlgorithmProvider(GenericMath<T> TMath)
        {
            this.TMath = TMath;
        }

        public void UpdateParams(AlgorithmParams Params)
        {
            this.Params = Params;
            ParamsUpdated();
        }

        public abstract PixelData Run(T px, T py);
        protected virtual void ParamsUpdated() { }
    }
}
