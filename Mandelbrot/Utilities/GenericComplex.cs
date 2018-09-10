using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Utilities
{
    struct GenericComplex<T>
    {
        public T r { get; }
        public T i { get; }

        public GenericComplex(T real, T imaginary)
        {
            r = real;
            i = imaginary;
        }
    }
}
