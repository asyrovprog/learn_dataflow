using System.Threading.Tasks.Dataflow;

namespace Lab.Iter02;

public record LogEntry(DateTime Timestamp, string Level, string Message);

public class LogStatistics
{
    public int TotalBatches { get; set; }
    public int TotalEntries { get; set; }
    public int LargestBatchSize { get; set; }
}

public static class LogAggregator
{
    public static async Task<LogStatistics> RunAsync(
        IEnumerable<LogEntry> logEntries,
        CancellationToken cancellationToken = default)
    {
        var stats = new LogStatistics();
        
        var (batchBlock, timer) = CreateBatchingPipeline(cancellationToken);
        var processor = CreateBatchProcessor(stats, cancellationToken);
        
        batchBlock.LinkTo(processor, new DataflowLinkOptions { PropagateCompletion = true });
        
        foreach (var entry in logEntries)
        {
            await batchBlock.SendAsync(entry, cancellationToken);
        }
        
        batchBlock.TriggerBatch();
        batchBlock.Complete();
        
        await processor.Completion;
        timer.Dispose();
        
        return stats;
    }
    
    private static (BatchBlock<LogEntry>, Timer) CreateBatchingPipeline(CancellationToken cancellationToken)
    {
        // TODO[N1]: Create a BatchBlock<LogEntry> with batch size 50 and a Timer that triggers batches every 5 seconds
        var batch = new BatchBlock<LogEntry>(50, new GroupingDataflowBlockOptions
        {
            BoundedCapacity = 5000,
            CancellationToken = cancellationToken
        });

        var timer = new Timer(_ => batch.TriggerBatch(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        return (batch, timer);
    }
    
    private static ActionBlock<LogEntry[]> CreateBatchProcessor(LogStatistics stats, CancellationToken cancellationToken)
    {
        // TODO[N2]: Create an ActionBlock that processes batches and updates statistics
        var action = new ActionBlock<LogEntry[]>((entries) =>
        {
            stats.TotalBatches++;
            stats.TotalEntries += entries.Length;
            stats.LargestBatchSize = Math.Max(stats.LargestBatchSize, entries.Length);

        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 10,
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 1,
        });
        return action;
    }
}