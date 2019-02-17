using MandelbrotSharp.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Utilities
{
    public class PointMapper<T>
    {
        private GenericMath<T> TMath;
        private T inXMin, inXMax, inYMin, inYMax;
        private T outXMin, outXMax, outYMin, outYMax;

        public PointMapper(object TMath)
        {
            this.TMath = TMath as GenericMath<T>;
        }

        public void SetInputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            inXMin = TMath.fromBigDecimal(xMin);
            inXMax = TMath.fromBigDecimal(xMax);
            inYMin = TMath.fromBigDecimal(yMin);
            inYMax = TMath.fromBigDecimal(yMax);
        }
        public void SetOutputSpace(BigDecimal xMin, BigDecimal xMax, BigDecimal yMin, BigDecimal yMax)
        {
            outXMin = TMath.fromBigDecimal(xMin);
            outXMax = TMath.fromBigDecimal(xMax);
            outYMin = TMath.fromBigDecimal(yMin);
            outYMax = TMath.fromBigDecimal(yMax);
        }
        public T MapPointX(double x)
        {
            T real = Utils.Map<T>(TMath, TMath.fromDouble(x), inXMin, inXMax, outXMin, outXMax);
            return real;
        }
        public T MapPointY(double y)
        {
            T imag = Utils.Map<T>(TMath, TMath.fromDouble(y), inYMin, inYMax, outYMin, outYMax);
            return imag;
        }
    }
}
