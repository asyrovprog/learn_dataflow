using Lab.Iter02;

var logEntries = new List<LogEntry>();

// Generate 125 log entries (should create 3 batches: 50, 50, 25)
for (int i = 1; i <= 125; i++)
{
    logEntries.Add(new LogEntry(
        DateTime.UtcNow.AddSeconds(i),
        i % 3 == 0 ? "ERROR" : "INFO",
        $"Log message {i}"
    ));
}

try
{
    var stats = await LogAggregator.RunAsync(logEntries);
    
    AssertEqual(3, stats.TotalBatches, "Total batches mismatch");
    AssertEqual(125, stats.TotalEntries, "Total entries mismatch");
    AssertEqual(50, stats.LargestBatchSize, "Largest batch size mismatch");
    
    Console.WriteLine("PASS: Log aggregator batching works correctly!");
    return 0;
}
catch (NotImplementedException ex)
{
    Console.Error.WriteLine($"FAIL: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL: Unexpected exception -> {ex.Message}");
    return 1;
}

static void AssertEqual<T>(T expected, T actual, string message)
    where T : IEquatable<T>
{
    if (!Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message} (expected {expected}, actual {actual})");
    }
}
