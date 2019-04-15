using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using MandelbrotSharp.Extras;
using MandelbrotSharp.Imaging;
using MandelbrotSharp.Rendering;

namespace Mandelbrot.Movies
{
    class ZoomMovieSettings : HistogramRenderSettings
    {
        private int _numFrames;
        private string _palettePath;
        private string _videoPath;
        private bool _extraPrecision;
        private int _version;

        [ScriptIgnore]
        public override Type AlgorithmType { get => base.AlgorithmType; set => base.AlgorithmType = value; }
        [ScriptIgnore]
        public override Type ArithmeticType { get => base.ArithmeticType; set => base.ArithmeticType = value; }
        [ScriptIgnore]
        public override RgbaValue[] Palette { get => base.Palette; set => base.Palette = value; }

        public int NumFrames { get => _numFrames; set => _numFrames = value; }

        public string PalettePath { get => _palettePath; set => _palettePath = value; }
        public string VideoPath { get => _videoPath; set => _videoPath = value; }

        public bool ExtraPrecision { get => _extraPrecision; set => _extraPrecision = value; }
        public int Version { get => _version; set => _version = value; }
    }
}
