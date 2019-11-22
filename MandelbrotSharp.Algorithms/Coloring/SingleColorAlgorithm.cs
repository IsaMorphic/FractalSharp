using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MandelbrotSharp.Algorithms.Coloring
{
    public class SingleColorAlgorithm : AlgorithmProvider<PointData, double, EmptyColoringParams>
    {
        protected override bool Initialize(CancellationToken cancellationToken) => true;
        public override double Run(PointData data) => 0.0;
    }
}
