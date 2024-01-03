using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FractalSharp.Algorithms.Coloring
{
    public class SingleColorAlgorithm : IAlgorithmProvider<PointData, double, EmptyColoringParams>
    {
        public static double Run(EmptyColoringParams @params, PointData data) => 0.0;
    }
}
