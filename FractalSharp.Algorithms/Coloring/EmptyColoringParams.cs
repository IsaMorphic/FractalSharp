using System;
using System.Collections.Generic;
using System.Text;

namespace FractalSharp.Algorithms.Coloring
{
    public class EmptyColoringParams : IAlgorithmParams
    {
        public IAlgorithmParams Copy()
        {
            return new EmptyColoringParams();
        }
    }
}
