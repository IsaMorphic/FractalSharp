using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Mandelbrot.Rendering;

namespace Mandelbrot.Movies
{
    class ZoomMovieSettings : RenderSettings
    {
        private int _numFrames;
        private string _palettePath;
        private string _videoPath;
        private bool _extraPrecision;
        private int _version;

        [ScriptIgnore]
        public override Type AlgorithmType { get => base.AlgorithmType; set => base.AlgorithmType = value; }

        public int NumFrames { get => _numFrames; set => _numFrames = value; }

        public string PalettePath { get => _palettePath; set => _palettePath = value; }
        public string VideoPath { get => _videoPath; set => _videoPath = value; }

        public bool ExtraPrecision { get => _extraPrecision; set => _extraPrecision = value; }
        public int Version { get => _version; set => _version = value; }
    }
}
