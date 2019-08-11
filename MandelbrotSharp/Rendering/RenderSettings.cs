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
using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;

namespace MandelbrotSharp.Rendering
{
    public class RenderSettings<TNumber> where TNumber : struct
    {
        private AlgorithmParams<TNumber> _params = new AlgorithmParams<TNumber>();

        private int _threadCount = Environment.ProcessorCount;

        private Gradient _outerColors;
        private RgbaValue _innerColor;

        public virtual AlgorithmParams<TNumber> Params { get => _params; set => _params = value; }

        public virtual int ThreadCount { get => _threadCount; set => _threadCount = value; }

        public virtual Gradient OuterColors { get => _outerColors; set => _outerColors = value; }
        public virtual RgbaValue InnerColor { get => _innerColor; set => _innerColor = value; }


        public virtual int MaxIterations { get => _params.MaxIterations; set => _params.MaxIterations = value; }

        public virtual Number<TNumber> Magnification { get => _params.Magnification; set => _params.Magnification = value; }
        public virtual Complex<TNumber> Location { get => _params.Location; set => _params.Location = value; }

        public virtual Dictionary<string, object> ExtraParams { get => _params.ExtraParams; set => _params.ExtraParams = value; }
    }
}
