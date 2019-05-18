/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of an example application that demostrates 
 *  the usage of the MandelbrotSharp library.
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
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot.Movies
{
    class MandelbrotMovieRenderer : MandelbrotRenderer
    {

        public int NumFrames { get; private set; }
        public int MaxIterations { get => base.MaxIterations; }
        public BigDecimal Magnification { get => base.Magnification; }

        public void Setup(ZoomMovieSettings settings)
        {
            base.Setup(settings);
            NumFrames = settings.NumFrames;
            base.Magnification = BigDecimal.Pow(2, NumFrames / 30.0);
        }

        public void SetFrame(int frameNum)
        {
            // Set variables and get new zoom value.  
            NumFrames = frameNum;

            base.Magnification = Math.Pow(2, NumFrames / 30.0);
        }
    }
}
