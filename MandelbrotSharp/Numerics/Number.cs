using MiscUtil;
using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Numerics
{
    public interface INumber
    {
        T GetValueAs<T>();
    }
    public class Number<T> : INumber
    {
        public T Value;
        
        public Number(T v)
        {
            Value = v;
        }

        public TOut GetValueAs<TOut>()
        {
            return Operator.Convert<T, TOut>(Value);
        }
    }
}
