using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Mathematics
{
    public struct GenericComplex<T>
    {
        public T real { get; }
        public T imag { get; }

        public GenericComplex(T real, T imag)
        {
            this.real = real;
            this.imag = imag;
        }
    }
}
