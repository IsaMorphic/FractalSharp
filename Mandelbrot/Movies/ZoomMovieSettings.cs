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
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mandelbrot.Movies
{
    class ZoomMovieSettings : RenderSettings
    {
        private int _numFrames;
        private string _palettePath;
        private string _videoPath;
        private bool _extraPrecision;
        private int _version;
        private int _width;
        private int _height;

        [ScriptIgnore]
        public override Type AlgorithmType { get => base.AlgorithmType; set => base.AlgorithmType = value; }
        [ScriptIgnore]
        public override Type ArithmeticType { get => base.ArithmeticType; set => base.ArithmeticType = value; }
        [ScriptIgnore]
        public override Type PixelColoratorType { get => base.PixelColoratorType; set => base.PixelColoratorType = value; }
        [ScriptIgnore]
        public override RgbaValue[] Palette { get => base.Palette; set => base.Palette = value; }

        public int NumFrames { get => _numFrames; set => _numFrames = value; }

        public string PalettePath { get => _palettePath; set => _palettePath = value; }
        public string VideoPath { get => _videoPath; set => _videoPath = value; }

        public bool ExtraPrecision { get => _extraPrecision; set => _extraPrecision = value; }
        public int Version { get => _version; set => _version = value; }

        public int Width { get => _width; set => _width = value; }
        public int Height { get => _height; set => _height = value; }
    }
}
