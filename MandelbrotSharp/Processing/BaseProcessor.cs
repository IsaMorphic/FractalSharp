
using MandelbrotSharp.Algorithms;
using System.Threading;
using System.Threading.Tasks;

namespace MandelbrotSharp.Processing
{
    public interface IProcessor<TOutput>
    {
        int Width { get; }
        int Height { get; }

        Task SetupAsync(ProcessorConfig settings, CancellationToken cancellationToken);
        Task<TOutput[,]> ProcessAsync(CancellationToken cancellationToken);
    }

    public abstract class BaseProcessor<TInput, TOutput, TAlgorithm> : IProcessor<TOutput>
        where TAlgorithm : IAlgorithmProvider<TInput, TOutput>, new()
    {
        public BaseProcessor(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        protected TAlgorithm AlgorithmProvider { get; private set; }

        protected ProcessorConfig Settings { get; private set; }

        public virtual async Task SetupAsync(ProcessorConfig settings, CancellationToken cancellationToken)
        {
            Settings = settings.Copy();
            AlgorithmProvider = new TAlgorithm();
            await AlgorithmProvider.Initialize(Settings.Params.Copy(), cancellationToken);
        }

        public Task<TOutput[,]> ProcessAsync(CancellationToken cancellationToken)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Settings.ThreadCount,
                CancellationToken = cancellationToken
            };
            return Task.Run(() => Process(options));
        }

        protected abstract TOutput[,] Process(ParallelOptions options);
    }
}
