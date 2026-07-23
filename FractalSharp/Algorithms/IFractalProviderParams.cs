using FractalSharp.Numerics.Generic;
using System.Numerics;

namespace FractalSharp.Algorithms
{
    public interface IFractalProviderParams<TNumber>
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>
    {
        int MaxIterations { get; set; }

        Complex<TNumber> Position { get; set; }

        TNumber Scale { get; set; }
    }
}
