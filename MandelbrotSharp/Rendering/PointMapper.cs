/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
 *
 *  MandelbrotSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MandelbrotSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MandelbrotSharp.  If not, see <https://www.gnu.org/licenses/>.
 */
using MandelbrotSharp.Numerics;

namespace MandelbrotSharp.Rendering
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

        public static Number<T> MapValue(Number<T> OldValue, Number<T> OldMin, Number<T> OldMax, Number<T> NewMin, Number<T> NewMax)
        {
            T OldRange = OldMax - OldMin;
            T NewRange = NewMax - NewMin;
            T NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }

        private Number<T> MapPointX(double x)
        {
            Number<T> real = MapValue(x, inXMin, inXMax, outXMin, outXMax);
            return real;
        }

        private Number<T> MapPointY(double y)
        {
            Number<T> imag = MapValue(y, inYMin, inYMax, outYMin, outYMax);
            return imag;
        }
    }
}
