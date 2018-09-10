using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Utilities
{
    struct ComplexMath<T, M> where M : IGenericMath<T>
    {
        private M _TMath;

        public ComplexMath(M TMath)
        {
            _TMath = TMath;
        }

        public GenericComplex<T> Add(GenericComplex<T> a, GenericComplex<T> b)
        {
            return new GenericComplex<T>(_TMath.Add(a.r, b.r), _TMath.Add(a.i, b.i));
        }

        public GenericComplex<T> Multiply(GenericComplex<T> a, GenericComplex<T> b)
        {
            T new_real = _TMath.Subtract(_TMath.Multiply(a.r, b.r), _TMath.Multiply(a.i, b.i));
            T new_imag = _TMath.Add(_TMath.Multiply(a.r, b.i), _TMath.Multiply(a.i, b.r));
            return new GenericComplex<T>(new_real, new_imag);
        }

        public GenericComplex<T> Multiply(GenericComplex<T> a, T b)
        {
            return new GenericComplex<T>(_TMath.Multiply(a.r, b), _TMath.Multiply(a.i, b));
        }

        public T MagnitudeSquared(GenericComplex<T> a)
        {
            return _TMath.Add(_TMath.Multiply(a.r, a.r), _TMath.Multiply(a.i, a.i));
        }
    }
}
