using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FractalSharp.Algorithms.Coloring
{
    public class SingleColorAlgorithm : AlgorithmProvider<PointData, double, EmptyColoringParams>
    {
        protected override bool Initialize(CancellationToken cancellationToken) => true;
        public override double Run(PointData data) => 0.0;
    }
}
