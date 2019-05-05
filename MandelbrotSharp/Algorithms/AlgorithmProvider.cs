using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider
    {
        void UpdateParams(AlgorithmParams Params);
        PixelData Run(dynamic px, dynamic py);
    }
    public abstract class AlgorithmProvider<T> : IAlgorithmProvider
    {
        protected AlgorithmParams Params { get; private set; }

        void IAlgorithmProvider.UpdateParams(AlgorithmParams Params)
        {
            this.Params = Params;
            ParamsUpdated();
        }

        PixelData IAlgorithmProvider.Run(dynamic px, dynamic py)
        {
            return Run(px, py);
        }

        public abstract PixelData Run(T px, T py);
        protected virtual void ParamsUpdated() { }
    }
}
