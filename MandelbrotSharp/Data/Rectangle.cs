using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MandelbrotSharp.Data
{
    public struct Rectangle<TNumber> where TNumber : struct
    {
        public Number<TNumber> XMin { get; }
        public Number<TNumber> XMax { get; }
        public Number<TNumber> YMin { get; }
        public Number<TNumber> YMax { get; }

        public Rectangle(Number<TNumber> xMin, Number<TNumber> xMax, Number<TNumber> yMin, Number<TNumber> yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }
    }
}
