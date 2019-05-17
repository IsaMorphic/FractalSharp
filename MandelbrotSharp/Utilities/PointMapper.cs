using MandelbrotSharp.Numerics;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Utilities
{
    public interface IPointMapper
    {
        void SetInputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax);
        void SetOutputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax);
        INumber MapPointX(double x);
        INumber MapPointY(double y);
    }

    public class PointMapper<T> : IPointMapper where T : struct
    {
        private Number<T> inXMin, inXMax, inYMin, inYMax;
        private Number<T> outXMin, outXMax, outYMin, outYMax;

        void IPointMapper.SetInputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            inXMin = Number<T>.From(xMin);
            inXMax = Number<T>.From(xMax);
            inYMin = Number<T>.From(yMin);
            inYMax = Number<T>.From(yMax);
        }
        void IPointMapper.SetOutputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            outXMin = Number<T>.From(xMin);
            outXMax = Number<T>.From(xMax);
            outYMin = Number<T>.From(yMin);
            outYMax = Number<T>.From(yMax);
        }

        INumber IPointMapper.MapPointX(double x)
        {
            return MapPointX(x);
        }

        INumber IPointMapper.MapPointY(double y)
        {
            return MapPointY(y);
        }

        private Number<T> MapPointX(double x)
        {
            Number<T> real = Utils.Map(x, inXMin, inXMax, outXMin, outXMax);
            return real;
        }

        private Number<T> MapPointY(double y)
        {
            Number<T> imag = Utils.Map(y, inYMin, inYMax, outYMin, outYMax);
            return imag;
        }
    }
}
