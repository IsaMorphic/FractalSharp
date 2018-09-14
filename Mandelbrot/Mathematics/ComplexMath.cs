using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Mathematics
{
    class ComplexMath<T>
    {
        private IGenericMath<T> TMath;

        public ComplexMath(IGenericMath<T> TMath)
        {
            this.TMath = TMath;
        }

        public GenericComplex<T> Add(GenericComplex<T> a, GenericComplex<T> b)
        {
            return new GenericComplex<T>(TMath.Add(a.real, b.real), TMath.Add(a.imag, b.imag));
        }

        public GenericComplex<T> Multiply(GenericComplex<T> a, GenericComplex<T> b)
        {
            T new_real = TMath.Subtract(TMath.Multiply(a.real, b.real), TMath.Multiply(a.imag, b.imag));
            T new_imag = TMath.Add(TMath.Multiply(a.real, b.imag), TMath.Multiply(a.imag, b.real));
            return new GenericComplex<T>(new_real, new_imag);
        }

        public GenericComplex<T> Multiply(GenericComplex<T> a, T b)
        {
            return new GenericComplex<T>(TMath.Multiply(a.real, b), TMath.Multiply(a.imag, b));
        }

        public T MagnitudeSquared(GenericComplex<T> a)
        {
            return TMath.Add(TMath.Multiply(a.real, a.real), TMath.Multiply(a.imag, a.imag));
        }
    }
}
