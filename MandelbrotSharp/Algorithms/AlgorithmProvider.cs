using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider
    {
        event EventHandler ParamsUpdated;
        void UpdateParams(AlgorithmParams Params);
        PixelData Run(dynamic px, dynamic py);
    }
    public abstract class AlgorithmProvider<T> : IAlgorithmProvider
    {
        protected AlgorithmParams Params { get; private set; }
        protected event EventHandler ParamsUpdated;

        event EventHandler IAlgorithmProvider.ParamsUpdated
        {
            add => ParamsUpdated += value;
            remove => ParamsUpdated -= value;
        }

        void IAlgorithmProvider.UpdateParams(AlgorithmParams Params)
        {
            this.Params = Params;
            OnParamsUpdated();
        }

        PixelData IAlgorithmProvider.Run(dynamic px, dynamic py)
        {
            return Run(px, py);
        }

        public abstract PixelData Run(T px, T py);

        protected virtual TOutput GetExtraParamValue<TOutput>(string name, TOutput def)
        {
            if (Params.ExtraParams.TryGetValue(name, out object val))
                return Operator.Convert<object, TOutput>(val);
            else
                return def;
        }

        protected virtual void OnParamsUpdated()
        {
            ParamsUpdated?.Invoke(this, null);
        }
    }
}
