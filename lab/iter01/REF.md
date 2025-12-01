# Lab Reference – Text Analytics Dataflow

<!-- markdownlint-disable MD033 -->

Use these hints only if you get stuck. Open the reference solution after you finish implementing the TODOs.

## TODO N1 – Build Metrics Transform (Hints)

- `Regex.Matches` with the pattern `"[A-Za-z]+"` is handy for stripping punctuation.
- Use `TransformBlock<string, SentenceMetrics>` and configure `MaxDegreeOfParallelism`.
- Consider returning immutable collections (e.g., `ToArray()` or `ToImmutableArray()`) for the distinct words list.

## TODO N2 – Aggregate Metrics (Hints)

- `ActionBlock<T>` can enforce sequential aggregation when `MaxDegreeOfParallelism = 1`.
- Remember to set `BoundedCapacity` and pass through the cancellation token.
- `PipelineSummary.AddSentenceMetrics` handles the bookkeeping—call it for each message.

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
// Task.cs (excerpt)
private static TransformBlock<string, SentenceMetrics> CreateMetricsBlock(CancellationToken cancellationToken)
{
	var options = new ExecutionDataflowBlockOptions
	{
		CancellationToken = cancellationToken,
		MaxDegreeOfParallelism = 4,
		EnsureOrdered = false
	};

	return new TransformBlock<string, SentenceMetrics>(sentence =>
	{
		var input = sentence ?? string.Empty;
		var matches = Regex.Matches(input, "[A-Za-z]+", RegexOptions.CultureInvariant);

		if (matches.Count == 0)
		{
			return new SentenceMetrics(0, Array.Empty<string>());
		}

		var distinctWords = matches
			.Select(match => match.Value.ToLowerInvariant())
			.Distinct()
			.ToArray();

		return new SentenceMetrics(matches.Count, distinctWords);
	}, options);
}

private static ActionBlock<SentenceMetrics> CreateAggregationBlock(PipelineSummary summary, CancellationToken cancellationToken)
{
	var options = new ExecutionDataflowBlockOptions
	{
		CancellationToken = cancellationToken,
		BoundedCapacity = 8,
		MaxDegreeOfParallelism = 1
	};

	return new ActionBlock<SentenceMetrics>(metrics =>
	{
		summary.AddSentenceMetrics(metrics);
	}, options);
}
```

</details>
