using Lab.Iter01;

var sentences = new[]
{
    "Dataflow pipelines make concurrent programming easier.",
    "Blocks can buffer, transform, and consume messages.",
    "Backpressure prevents downstream overload.",
    "TPL Dataflow integrates naturally with async workflows."
};

try
{
    var summary = await TextAnalyticsPipeline.RunPipelineAsync(sentences);

    AssertEqual(4, summary.SentenceCount, "Sentence count mismatch");
    AssertEqual(24, summary.TotalWordCount, "Total word count mismatch");
    AssertEqual(23, summary.DistinctWordCount, "Distinct word count mismatch");
    AssertEqual(7, summary.MaxDistinctWordsInSentence, "Max distinct words mismatch");

    var expectedWords = new[]
    {
        "dataflow", "pipelines", "make", "concurrent", "programming", "easier",
        "blocks", "can", "buffer", "transform", "and", "consume", "messages",
        "backpressure", "prevents", "downstream", "overload", "tpl", "integrates",
        "naturally", "with", "async", "workflows"
    };

    foreach (var word in expectedWords)
    {
        if (!summary.DistinctWords.Contains(word))
        {
            throw new InvalidOperationException($"Expected word '{word}' missing from distinct set.");
        }
    }

    Console.WriteLine("PASS: Text analytics dataflow summary looks great!");
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