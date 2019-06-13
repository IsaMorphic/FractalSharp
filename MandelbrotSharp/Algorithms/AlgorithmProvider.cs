/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using MiscUtil;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider
    {
        void UpdateParams(AlgorithmParams Params);
        void Initialize(object state);
        PointData Run(INumber px, INumber py);
    }
    public abstract class AlgorithmProvider<T> : IAlgorithmProvider where T : struct
    {
        protected AlgorithmParams Params { get; private set; }

        void IAlgorithmProvider.UpdateParams(AlgorithmParams Params)
        {
            this.Params = Params;
            Type t = GetType();
            FieldInfo[] fields = t.GetFields();
            foreach (var field in fields)
            {
                var attr = (ParameterAttribute)field.GetCustomAttribute(typeof(ParameterAttribute));
                if (attr != null)
                {
                    object val = GetExtraParamValue(field.Name, attr.DefaultValue);
                    Type Operator = typeof(Operator);
                    Type[] targs = new Type[] { val.GetType(), field.FieldType };
                    MethodInfo converter = Operator.GetMethod("Convert").MakeGenericMethod(targs);
                    field.SetValue(this, converter.Invoke(null, new object[] { val }));
                }
            }
            OnParamsUpdated();
        }

        PointData IAlgorithmProvider.Run(INumber px, INumber py)
        {
            return Run(px.As<T>(), py.As<T>());
        }

        void IAlgorithmProvider.Initialize(object state)
        {
            Initialize((CancellationToken)state);
        }

        protected abstract PointData Run(Number<T> px, Number<T> py);

        protected virtual void Initialize(CancellationToken token) { }

        protected virtual void OnParamsUpdated() { }

        private TOutput GetExtraParamValue<TOutput>(string name, TOutput def)
        {
            if (Params.ExtraParams.TryGetValue(name, out object val))
                return Operator.Convert<object, TOutput>(val);
            else
                return def;
        }
    }
}
