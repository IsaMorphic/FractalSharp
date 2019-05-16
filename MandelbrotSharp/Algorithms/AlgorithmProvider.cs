using MandelbrotSharp.Imaging;
using System.Reflection;
using MandelbrotSharp.Rendering;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider
    {
        event EventHandler ParamsUpdated;
        void UpdateParams(AlgorithmParams Params);
        PixelData Run(INumber px, INumber py);
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
            Type t = GetType();
            FieldInfo[] fields = t.GetFields();
            foreach (var field in fields)
            {
                var attr = (ParameterAttribute)field.GetCustomAttribute(typeof(ParameterAttribute));
                if (attr != null)
                    field.SetValue(this, GetExtraParamValue(field.Name, attr.DefaultValue));
            }
            OnParamsUpdated();
        }

        PixelData IAlgorithmProvider.Run(INumber px, INumber py)
        {
            Number<T> x = (Number<T>)px;
            Number<T> y = (Number<T>)py;
            return Run(x.Value, y.Value);
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
