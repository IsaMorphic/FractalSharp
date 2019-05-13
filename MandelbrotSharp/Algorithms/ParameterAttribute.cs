using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Algorithms
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class ParameterAttribute : System.Attribute
    {
        public object DefaultValue;
    }
}
