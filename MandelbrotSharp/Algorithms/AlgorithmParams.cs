﻿/*
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
using System.Collections.Generic;

namespace MandelbrotSharp.Algorithms
{
    public class AlgorithmParams
    {
        private int _maxIterations = 100;
        private BigDecimal _magnification = 1;
        private Complex<BigDecimal> _location;

        private Dictionary<string, object> _extraParams = new Dictionary<string, object>();

        public virtual BigDecimal Magnification { get => _magnification; set => _magnification = value; }
        public virtual int MaxIterations { get => _maxIterations; set => _maxIterations = value; }
        public virtual Dictionary<string, object> ExtraParams { get => _extraParams; set => _extraParams = value; }
        public virtual Complex<BigDecimal> Location { get => _location; set => _location = value; }
    }
}
