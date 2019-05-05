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
        dynamic MapPointX(double x);
        dynamic MapPointY(double y);
    }

    public class PointMapper<T> : IPointMapper
    {
        private T inXMin, inXMax, inYMin, inYMax;
        private T outXMin, outXMax, outYMin, outYMax;

        void IPointMapper.SetInputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            inXMin = Operator.Convert<BigDecimal, T>(xMin);
            inXMax = Operator.Convert<BigDecimal, T>(xMax);
            inYMin = Operator.Convert<BigDecimal, T>(yMin);
            inYMax = Operator.Convert<BigDecimal, T>(yMax);
        }
        void IPointMapper.SetOutputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            outXMin = Operator.Convert<BigDecimal, T>(xMin);
            outXMax = Operator.Convert<BigDecimal, T>(xMax);
            outYMin = Operator.Convert<BigDecimal, T>(yMin);
            outYMax = Operator.Convert<BigDecimal, T>(yMax);
        }

        dynamic IPointMapper.MapPointX(double x)
        {
            return MapPointX(x);
        }

        dynamic IPointMapper.MapPointY(double y)
        {
            return MapPointY(y);
        }

        private T MapPointX(double x)
        {
            T real = Utils.Map(Operator.Convert<double, T>(x), inXMin, inXMax, outXMin, outXMax);
            return real;
        }

        private T MapPointY(double y)
        {
            T imag = Utils.Map(Operator.Convert<double, T>(y), inYMin, inYMax, outYMin, outYMax);
            return imag;
        }
    }
}
