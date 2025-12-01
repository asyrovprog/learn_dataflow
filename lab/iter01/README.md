# Text Analytics Dataflow Lab

This lab reinforces the fundamentals of the **TPL Dataflow** library by asking you to build a lightweight text analytics pipeline. You will wire together dataflow blocks that transform input sentences into metrics and then aggregate the results to compute summary statistics.

## Scenario

- A stream of sentences arrives for processing.
- You will create a metrics block that normalizes each sentence, splits it into words, and emits per-sentence metrics.
- You will aggregate these metrics to produce an overall summary containing totals and distinct word counts.

When the TODOs are complete, running `dotnet run` inside `lab/iter01/` should print `PASS` for all checks.

### Running the Lab

```bash
cd lab/iter01
dotnet run
```

If a TODO is still unfinished, the program will throw an exception that references the missing work.

---

## TODO N1 – Build Metrics Transform

Implement `CreateMetricsBlock` in `Task.cs` so that it returns a `TransformBlock<string, SentenceMetrics>` which:

- Normalizes text to lower case, extracts words, and filters out empties.
- Emits a `SentenceMetrics` instance containing the full list of distinct words and the word count for each sentence.
- Supports limited parallelism (`MaxDegreeOfParallelism = 4`) and honors cancellation.

Key APIs to consider: `Regex.Matches`, `TransformBlock`, and `ExecutionDataflowBlockOptions`.

---

## TODO N2 – Aggregate Metrics

Implement `CreateAggregationBlock` in `Task.cs` so that it returns an `ActionBlock<SentenceMetrics>` which:

- Updates the provided `PipelineSummary` by tracking sentence count, total words, maximum distinct words in a single sentence, and the global distinct word set.
- Uses `BoundedCapacity = 8` to demonstrate backpressure.
- Propagates completion and honors cancellation so the pipeline can shut down cleanly.

Useful members: `PipelineSummary.AddSentenceMetrics`, `ActionBlock`, and `ExecutionDataflowBlockOptions`.

---

## Reference

Hints and a reference solution live in `REF.md`. Open the solution only after you finish your implementation.
