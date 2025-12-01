# Log Aggregator with Time-Based Batching Lab

This lab reinforces **BatchBlock** fundamentals by building a log aggregation pipeline that batches log entries based on count OR time—whichever comes first.

## Scenario

You're building a high-throughput logging system where:
- Log entries arrive continuously
- Entries should be written to disk in batches to reduce I/O overhead
- Batches trigger when 50 entries accumulate OR every 5 seconds (prevents delays)
- You need to handle shutdown gracefully (flush incomplete batches)

When complete, running `dotnet run` should print `PASS` for all tests.

### Running the Lab

``bash
cd lab/iter02
dotnet run
``

---

## TODO N1 – Create BatchBlock with Timer

Implement `CreateBatchingPipeline` in `Task.cs` to:

- Create a `BatchBlock<LogEntry>` with batch size of 50
- Set up a `Timer` that calls `TriggerBatch()` every 5 seconds
- Configure appropriate options (cancellation, bounded capacity)
- Return both the `BatchBlock` and `Timer` for lifecycle management

Key concepts: `BatchBlock`, `TriggerBatch()`, `Timer`, `GroupingDataflowBlockOptions`

---

## TODO N2 – Implement Batch Processor

Implement `CreateBatchProcessor` in `Task.cs` to:

- Create an `ActionBlock<LogEntry[]>` that processes batches
- Track statistics: total batches processed, total entries, largest batch size
- Write batch summary to console
- Configure sequential processing (one batch at a time)

Key concepts: `ActionBlock`, array processing, statistics tracking

---

## Reference

Hints and the reference solution are in `REF.md`. Open the solution only after completing your implementation.
