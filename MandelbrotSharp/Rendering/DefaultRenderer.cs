using MandelbrotSharp.Algorithms;
using MandelbrotSharp.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotSharp.Rendering
{
    public class DefaultRenderer<TNumber, TAlgorithm> : FractalRenderer<TNumber, TAlgorithm>
        where TAlgorithm : IAlgorithmProvider<TNumber>, new()
        where TNumber : struct
    {
        public DefaultRenderer(int width, int height) : base(width, height)
        {
        }

        protected override void RenderFrame(ParallelOptions options)
        {
            Parallel.For(0, Height, options, py =>
            {
                var y0 = PointMapper.MapPointY(py);

                Parallel.For(0, Width, options, px =>
                {
                    var x0 = PointMapper.MapPointX(px);

                    PointData pointData = AlgorithmProvider.Run(new Complex<TNumber>(x0, y0));

                    if (pointData.Escaped)
                    {
                        double colorIndex = PointColorer.GetIndexFromPointData(pointData);
                        CurrentFrame.SetPixel(px, py, OuterColors[colorIndex]);
                    }
                    else
                    {
                        CurrentFrame.SetPixel(px, py, InnerColor);
                    }
                });
            });
        }
    }
}
