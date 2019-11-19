/*
 *  Copyright 2018-2019 Chosen Few Software
 *  This file is part of MandelbrotSharp.
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
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Algorithms
{
    public interface IAlgorithmProvider<TInput, TOutput>
    {
        bool Initialized { get; }
        Task Initialize(IFractalParams @params, CancellationToken token);
        TOutput Run(TInput data);
    }

    public abstract class AlgorithmProvider<TInput, TOutput, TParam> : IAlgorithmProvider<TInput, TOutput>
        where TParam : class, IAlgorithmParams
    {
        public bool Initialized { get; private set; }

        protected TParam Params { get; private set; }

        public async Task Initialize(IFractalParams @params, CancellationToken cancellationToken)
        {
            Params = @params as TParam;
            Initialized = await Task.Run(
                () => Initialize(cancellationToken)
                );
        }
        public abstract TOutput Run(TInput data);
        protected abstract bool Initialize(CancellationToken cancellationToken);
    }
}
