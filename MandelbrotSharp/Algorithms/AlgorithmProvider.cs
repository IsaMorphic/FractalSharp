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
using MandelbrotSharp.Numerics;
using MiscUtil;
using System;
using System.Reflection;
using System.Threading;

namespace MandelbrotSharp.Algorithms
{
    public abstract class AlgorithmProvider<TNumber> where TNumber : struct
    {
        protected AlgorithmParams<TNumber> Params { get; }

        public abstract PointData Run(Complex<TNumber> point);

        public AlgorithmProvider(AlgorithmParams<TNumber> @params)
        {
            Params = @params;

            Type t = GetType();
            Type Operator = typeof(Operator);

            FieldInfo[] fields = t.GetFields();
            foreach (var field in fields)
            {
                var attr = (ParameterAttribute)field.GetCustomAttribute(typeof(ParameterAttribute));
                if (attr != null)
                {
                    object val = GetExtraParamValue(field.Name, attr.DefaultValue);
                    Type[] targs = new Type[] { val.GetType(), field.FieldType };
                    MethodInfo converter = Operator.GetMethod("Convert").MakeGenericMethod(targs);
                    field.SetValue(this, converter.Invoke(null, new object[] { val }));
                }
            }
        }

        private TOutput GetExtraParamValue<TOutput>(string name, TOutput def)
        {
            if (Params.ExtraParams.TryGetValue(name, out object val))
                return Operator.Convert<object, TOutput>(val);
            else
                return def;
        }
    }
}
