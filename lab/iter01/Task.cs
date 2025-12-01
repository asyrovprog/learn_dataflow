using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter01;

public record SentenceMetrics(int WordCount, IReadOnlyCollection<string> DistinctWords);

public sealed class PipelineSummary
{
    private readonly HashSet<string> _distinctWords = new(StringComparer.OrdinalIgnoreCase);

    public int SentenceCount { get; private set; }
    public int TotalWordCount { get; private set; }
    public int MaxDistinctWordsInSentence { get; private set; }
    public int DistinctWordCount => _distinctWords.Count;
    public double AverageWordsPerSentence => SentenceCount == 0 ? 0 : (double)TotalWordCount / SentenceCount;
    public IReadOnlyCollection<string> DistinctWords => _distinctWords.ToImmutableArray();

    internal void AddSentenceMetrics(SentenceMetrics metrics)
    {
        SentenceCount++;
        TotalWordCount += metrics.WordCount;
        MaxDistinctWordsInSentence = Math.Max(MaxDistinctWordsInSentence, metrics.DistinctWords.Count);

        foreach (var word in metrics.DistinctWords)
        {
            _distinctWords.Add(word);
        }
    }
}

public static class TextAnalyticsPipeline
{
    public static async Task<PipelineSummary> RunPipelineAsync(IEnumerable<string> sentences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sentences);

        var summary = new PipelineSummary();
        var input = new BufferBlock<string>(new DataflowBlockOptions
        {
            CancellationToken = cancellationToken
        });

        var metrics = CreateMetricsBlock(cancellationToken);
        var aggregator = CreateAggregationBlock(summary, cancellationToken);

        input.LinkTo(metrics, new DataflowLinkOptions { PropagateCompletion = true });
        metrics.LinkTo(aggregator, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var sentence in sentences)
        {
            await input.SendAsync(sentence, cancellationToken).ConfigureAwait(false);
        }

        input.Complete();
        await aggregator.Completion.ConfigureAwait(false);
        return summary;
    }

    private static TransformBlock<string, SentenceMetrics> CreateMetricsBlock(CancellationToken cancellationToken)
    {
        // TODO[N1]: 
        // Build a transform block that normalizes text, 
        // extracts distinct words, and emits per-sentence metrics with constrained parallelism.

        var transform = new TransformBlock<string, SentenceMetrics>((s) =>
        {
            var words = ExtractWords(s);
            return new SentenceMetrics(words.Count, words);

        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            EnsureOrdered = false,
            CancellationToken = cancellationToken
        });
        return transform;
    }

    private static ActionBlock<SentenceMetrics> CreateAggregationBlock(PipelineSummary summary, CancellationToken cancellationToken)
    {
        var aggregator = new ActionBlock<SentenceMetrics>((metrics) =>
        {
            summary.AddSentenceMetrics(metrics);
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1,
            BoundedCapacity = 1024,
            EnsureOrdered = false,
        });

        return aggregator;
    }

    private static IReadOnlyCollection<string> ExtractWords(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            return Array.Empty<string>();
        }

        var matches = Regex.Matches(sentence, "[A-Za-z]+", RegexOptions.CultureInvariant);
        return matches.Select(match => match.Value.ToLowerInvariant()).Distinct().ToArray();
    }
}