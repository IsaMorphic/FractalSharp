using MandelbrotSharp.Data;
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Rendering
{
    public class PointMapper<TNumberIn, TNumberOut> where TNumberIn : struct where TNumberOut : struct
    {
        private Rectangle<TNumberIn> InputSpace { get; }
        private Rectangle<TNumberOut> OutputSpace { get; }

        public PointMapper(Rectangle<TNumberIn> regionIn, Rectangle<TNumberOut> regionOut)
        {
            InputSpace = regionIn;
            OutputSpace = regionOut;
        }

        public Number<TNumberOut> MapPointX(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(), 
                InputSpace.XMin.As<TNumberOut>(), InputSpace.XMax.As<TNumberOut>(), 
                OutputSpace.XMin, OutputSpace.XMax);
        }

        public Number<TNumberOut> MapPointY(Number<TNumberIn> value)
        {
            return MapValue(value.As<TNumberOut>(),
                InputSpace.YMin.As<TNumberOut>(), InputSpace.YMax.As<TNumberOut>(),
                OutputSpace.YMin, OutputSpace.YMax);
        }

        private static Number<TNumber> MapValue<TNumber>(Number<TNumber> OldValue, Number<TNumber> OldMin, Number<TNumber> OldMax, Number<TNumber> NewMin, Number<TNumber> NewMax) where TNumber : struct
        {
            Number<TNumber> OldRange = OldMax - OldMin;
            Number<TNumber> NewRange = NewMax - NewMin;
            Number<TNumber> NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }
    }
}
