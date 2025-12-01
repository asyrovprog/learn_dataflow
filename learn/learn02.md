# Learning BatchBlock & Grouping Data in TPL Dataflow

## What is BatchBlock?

`BatchBlock<T>` is a dataflow block that **collects individual messages into groups (batches)** before passing them downstream. Think of it like a shopping cart—you add items one by one, and when the cart reaches a certain size (or you decide to check out), everything moves forward together.

**Key Intuition:** Instead of processing messages one-at-a-time, you group them for bulk operations—reducing overhead, improving throughput, and enabling efficient batch APIs (like bulk database inserts).

## Why Use Batching?

### Performance Benefits

- **Reduced overhead**: One database transaction for 100 rows vs. 100 separate transactions
- **Better throughput**: Network calls benefit from batching (fewer round-trips)
- **Resource efficiency**: Allocate/deallocate resources once per batch instead of per item

### Real-World Examples

- Bulk inserting records into a database
- Sending batched notifications (email, SMS)
- Processing log entries in chunks
- Aggregating sensor readings for analysis

## Core Concepts

### 1. Batch Size

You configure how many items trigger a batch:

```csharp
var batchBlock = new BatchBlock<int>(batchSize: 10);
```

When 10 items arrive, `BatchBlock` outputs an array: `int[]` with 10 elements.

### 2. Greedy vs Non-Greedy Mode

**Greedy (default)**: Accepts all offered messages immediately and batches them.

```csharp
var greedyBatch = new BatchBlock<int>(5); // Accepts first 5 messages
```

**Non-Greedy**: Waits until it can collect exactly `batchSize` messages from **all** sources before accepting any.

```csharp
var nonGreedyBatch = new BatchBlock<int>(5, new GroupingDataflowBlockOptions 
{ 
    Greedy = false 
});
```

Non-greedy is useful for coordinating multiple sources—ensuring you get one message from each before proceeding.

### 3. Triggering a Batch Early

Sometimes you can't wait for a full batch (end-of-stream, timeout). Call `TriggerBatch()` to force an incomplete batch:

```csharp
batchBlock.TriggerBatch(); // Outputs whatever items are buffered
```

Common pattern: use a timer to trigger batches periodically.

## Simple Example: Batching Numbers

```csharp
using System.Threading.Tasks.Dataflow;

var batchBlock = new BatchBlock<int>(batchSize: 3);

var printBlock = new ActionBlock<int[]>(batch =>
{
    Console.WriteLine($"Batch of {batch.Length}: [{string.Join(", ", batch)}]");
});

batchBlock.LinkTo(printBlock, new DataflowLinkOptions { PropagateCompletion = true });

// Post 7 items
for (int i = 1; i <= 7; i++)
{
    batchBlock.Post(i);
}

// Trigger incomplete batch for remaining items
batchBlock.TriggerBatch();
batchBlock.Complete();

await printBlock.Completion;
```

**Output:**

```
Batch of 3: [1, 2, 3]
Batch of 3: [4, 5, 6]
Batch of 1: [7]
```

## Practical Example: Bulk Database Insert

```csharp
var batchBlock = new BatchBlock<Customer>(batchSize: 100);

var insertBlock = new ActionBlock<Customer[]>(async customers =>
{
    Console.WriteLine($"Bulk inserting {customers.Length} customers...");
    await database.BulkInsertAsync(customers);
    Console.WriteLine($"Inserted {customers.Length} records.");
}, new ExecutionDataflowBlockOptions 
{ 
    MaxDegreeOfParallelism = 1 // Sequential batches
});

batchBlock.LinkTo(insertBlock, new DataflowLinkOptions { PropagateCompletion = true });

// Stream customers from CSV
foreach (var customer in ReadCustomersFromCsv())
{
    await batchBlock.SendAsync(customer);
}

batchBlock.Complete();
await insertBlock.Completion;
```

Benefits:

- 1 transaction per 100 customers instead of 100 transactions
- Reduced network round-trips
- Better database performance

## Time-Based Batching Pattern

Sometimes you want batches based on **time** rather than count. Combine `BatchBlock` with a timer:

```csharp
var batchBlock = new BatchBlock<LogEntry>(batchSize: 50);
var timer = new Timer(_ => batchBlock.TriggerBatch(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

var logWriter = new ActionBlock<LogEntry[]>(async batch =>
{
    await File.AppendAllLinesAsync("log.txt", batch.Select(e => e.Message));
});

batchBlock.LinkTo(logWriter, new DataflowLinkOptions { PropagateCompletion = true });

// Logs are written either when 50 accumulate OR every 5 seconds
```

**Pattern**: Batch triggers on **whichever comes first**—size limit or timer.

## Configuration Options

### GroupingDataflowBlockOptions

```csharp
var options = new GroupingDataflowBlockOptions
{
    BoundedCapacity = 1000,        // Max items to buffer
    Greedy = true,                 // Accept messages immediately
    CancellationToken = token,     // Cancellation support
    MaxMessagesPerTask = 10        // Process batches in groups
};

var batchBlock = new BatchBlock<int>(50, options);
```

### Key Settings

- `BoundedCapacity`: Controls backpressure (blocks when full)
- `Greedy`: Coordination mode (true = immediate, false = wait for all sources)
- `MaxMessagesPerTask`: Performance tuning (how many batches per scheduler task)

## BatchBlock Output Type

`BatchBlock<T>` outputs `T[]` arrays:

```csharp
BatchBlock<string>  → outputs string[]
BatchBlock<int>     → outputs int[]
BatchBlock<MyClass> → outputs MyClass[]
```

Link to blocks that accept arrays:

```csharp
var batch = new BatchBlock<int>(10);
var process = new ActionBlock<int[]>(array => { /* work with array */ });
batch.LinkTo(process);
```

## Common Patterns

### Pattern 1: Batch-Transform-Unbatch

```csharp
var batchBlock = new BatchBlock<int>(10);
var processBlock = new TransformManyBlock<int[], int>(batch =>
{
    // Process batch, return individual results
    return batch.Select(x => x * 2);
});

batchBlock.LinkTo(processBlock);
```

### Pattern 2: Multiple Batch Sizes

```csharp
// Small batches for urgent items, large batches for bulk
var urgentBatch = new BatchBlock<Order>(5);
var bulkBatch = new BatchBlock<Order>(100);

// Route based on priority
order.IsUrgent ? urgentBatch.Post(order) : bulkBatch.Post(order);
```

### Pattern 3: Coordinated Batching (Non-Greedy)

```csharp
// Wait for 1 message from each of 3 sources before proceeding
var coordinator = new BatchBlock<Message>(3, new GroupingDataflowBlockOptions 
{ 
    Greedy = false 
});

source1.LinkTo(coordinator);
source2.LinkTo(coordinator);
source3.LinkTo(coordinator);

// Outputs Message[3] only when all 3 sources provide data
```

## Best Practices

✅ **Choose batch size wisely**

- Too small: minimal benefit
- Too large: increased latency, memory pressure
- Sweet spot: 50-500 for most scenarios

✅ **Handle incomplete batches**

- Always call `TriggerBatch()` before `Complete()`
- Use timers for time-based triggers

✅ **Consider backpressure**

- Set `BoundedCapacity` to prevent unbounded buffering
- Monitor memory usage with large batches

✅ **Error handling**

- One exception in batch processing can fail the entire batch
- Consider try-catch per item or batch-level retry logic

❌ **Avoid**

- Batching when order matters critically (use sequential processing)
- Very large batches that exceed memory limits
- Forgetting to trigger incomplete batches at shutdown

## When to Use BatchBlock

✅ **Good for:**

- Bulk database operations (insert, update, delete)
- Batched API calls (notifications, webhooks)
- Log aggregation and writing
- Reducing I/O overhead
- Network request batching

❌ **Not ideal for:**

- Real-time processing where latency matters
- Small datasets where batching adds complexity
- Scenarios requiring immediate per-item feedback

## Quick Reference

| Feature | Description |
|---------|-------------|
| `new BatchBlock<T>(size)` | Create batch block with size |
| `TriggerBatch()` | Force incomplete batch output |
| `Greedy = false` | Non-greedy coordination mode |
| `BoundedCapacity` | Limit buffered items |
| Output Type | `T[]` array of batched items |
