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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public class AlgorithmParams
    {
        private CancellationToken _token;

        private int _maxIterations = 100;
        private BigDecimal _magnification = 1;
        private BigDecimal _offsetX = BigDecimal.Parse("-743643887037158704752191506114774E-33");
        private BigDecimal _offsetY =  BigDecimal.Parse("131825904205311970493132056385139E-33");

        private Dictionary<string, object> _extraParams = new Dictionary<string, object>();

        public virtual CancellationToken Token { get => _token; set => _token = value; }

        public virtual BigDecimal Magnification { get => _magnification; set => _magnification = value; }
        public virtual BigDecimal offsetX { get => _offsetX; set => _offsetX = value; }
        public virtual BigDecimal offsetY { get => _offsetY; set => _offsetY = value; }
        public virtual int MaxIterations { get => _maxIterations; set => _maxIterations = value; }
        public virtual Dictionary<string, object> ExtraParams { get => _extraParams; set => _extraParams = value; }
    }
}
