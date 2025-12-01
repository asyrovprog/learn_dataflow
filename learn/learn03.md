# Learning Error Handling & Block Faults in TPL Dataflow

## Why Error Handling Matters in Dataflow

In real-world pipelines, things go wrong: network calls fail, data is invalid, databases disconnect. Unlike traditional try-catch, dataflow blocks process items asynchronously and independently—so **one failure can cascade through your entire pipeline** if not handled properly.

**Key Intuition:** Think of a factory assembly line. If one station breaks down, you need to decide: stop the whole line? Let other stations finish their work? Reroute defective items?

## Core Concepts

### 1. Block Fault States

Every dataflow block has a **Completion** property (a `Task`) that can end in three states:

``csharp
var block = new TransformBlock<int, int>(x => x * 2);

// Three possible outcomes:
await block.Completion; // ✅ RanToCompletion - success
// ❌ Faulted - an exception occurred
// ⚠️ Canceled - cancellation was requested
``

When an exception occurs inside a block, the block enters a **faulted state**.

### 2. Fault Propagation

By default, faults **propagate downstream** when you use `PropagateCompletion`:

``csharp
var source = new TransformBlock<int, int>(x =>
{
    if (x == 5) throw new InvalidOperationException("Bad number!");
    return x * 2;
});

var target = new ActionBlock<int>(x => Console.WriteLine(x));

source.LinkTo(target, new DataflowLinkOptions 
{ 
    PropagateCompletion = true // Faults propagate too!
});

source.Post(1); // ✅ Processes fine
source.Post(5); // ❌ Throws exception, faults source block
source.Complete();

try
{
    await target.Completion; // ❌ Also faults!
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Pipeline failed: {ex.Message}");
}
``

**What happened:**
1. Item `5` caused exception in `source`
2. `source` block faulted
3. Fault propagated to `target` via `PropagateCompletion`
4. Entire pipeline stopped

### 3. Catching Block Exceptions

To inspect the actual exception, check the `Completion` task:

``csharp
source.Complete();

try
{
    await source.Completion;
}
catch (AggregateException ae)
{
    foreach (var ex in ae.InnerExceptions)
    {
        Console.WriteLine($"Block faulted: {ex.Message}");
    }
}
``

**Important:** Exceptions are wrapped in `AggregateException` because blocks may process multiple items in parallel.

## Error Handling Strategies

### Strategy 1: Try-Catch Inside Block (Item-Level Recovery)

Handle exceptions **per item** so one failure doesn't kill the pipeline:

``csharp
var resilientBlock = new TransformBlock<string, int>(input =>
{
    try
    {
        return int.Parse(input); // May throw FormatException
    }
    catch (FormatException)
    {
        Console.WriteLine($"Invalid input '{input}', using default");
        return 0; // Default value
    }
});

resilientBlock.Post("123");  // ✅ Returns 123
resilientBlock.Post("abc");  // ⚠️ Logs warning, returns 0
resilientBlock.Post("456");  // ✅ Returns 456

resilientBlock.Complete();
await resilientBlock.Completion; // ✅ No fault!
``

**Pros:**
- Pipeline continues running
- Granular error handling per item
- Can log or recover from individual failures

**Cons:**
- Need to decide on default/fallback values
- Errors might be silently swallowed if not logged

### Strategy 2: Predicate Filtering (Route Bad Data)

Use link predicates to route invalid items to a separate error-handling path:

``csharp
var parser = new TransformBlock<string, int?>(input =>
{
    if (int.TryParse(input, out int result))
        return result;
    return null; // Signal failure with null
});

var validTarget = new ActionBlock<int?>(x => 
    Console.WriteLine($"Valid: {x}"));

var errorTarget = new ActionBlock<int?>(x => 
    Console.WriteLine($"Error: could not parse"));

// Route valid items
parser.LinkTo(validTarget, new DataflowLinkOptions 
{ 
    PropagateCompletion = true 
}, x => x.HasValue);

// Route invalid items
parser.LinkTo(errorTarget, new DataflowLinkOptions 
{ 
    PropagateCompletion = true 
}, x => !x.HasValue);

parser.Post("123"); // → validTarget
parser.Post("abc"); // → errorTarget
``

**Pros:**
- Clean separation of success/error paths
- No exceptions thrown
- Can process errors differently (retry, log, alert)

**Cons:**
- Requires designing error-signaling mechanism (null, Result<T>, etc.)
- More complex pipeline structure

### Strategy 3: Pipeline-Level Fault Monitoring

Monitor the pipeline completion and handle faults globally:

``csharp
var pipeline = BuildPipeline(); // Returns final block

pipeline.Complete();

try
{
    await pipeline.Completion;
    Console.WriteLine("✅ Pipeline completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Pipeline faulted: {ex.Message}");
    
    // Cleanup, logging, alerting
    await NotifyOpsTeam(ex);
    await FlushBuffers();
}
``

**Pros:**
- Simple centralized error handling
- Good for critical failures that should stop everything

**Cons:**
- Entire pipeline stops on first error
- Can't recover or continue processing

## Practical Example: Resilient Data Import Pipeline

``csharp
public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }
}

var parseBlock = new TransformBlock<string, ImportResult>(line =>
{
    try
    {
        var data = JsonSerializer.Deserialize<Customer>(line);
        return new ImportResult { Success = true, Data = data };
    }
    catch (JsonException ex)
    {
        return new ImportResult 
        { 
            Success = false, 
            ErrorMessage = $"Parse error: {ex.Message}" 
        };
    }
});

var successBlock = new ActionBlock<ImportResult>(async result =>
{
    await database.InsertAsync((Customer)result.Data!);
    successCount++;
});

var errorBlock = new ActionBlock<ImportResult>(result =>
{
    Console.Error.WriteLine($"❌ {result.ErrorMessage}");
    errorCount++;
});

parseBlock.LinkTo(successBlock, r => r.Success);
parseBlock.LinkTo(errorBlock, r => !r.Success);

// Process all lines, count successes and failures
foreach (var line in File.ReadLines("customers.json"))
{
    await parseBlock.SendAsync(line);
}

parseBlock.Complete();
await Task.WhenAll(successBlock.Completion, errorBlock.Completion);

Console.WriteLine($"Imported {successCount} records, {errorCount} errors");
``

## Fault Propagation Control

### Stopping Propagation

Don't set `PropagateCompletion` if you want faults isolated:

``csharp
source.LinkTo(target); // No propagation options
// If source faults, target keeps running
``

### Manual Fault Handling

``csharp
source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });

source.Complete();

source.Completion.ContinueWith(task =>
{
    if (task.IsFaulted)
    {
        Console.WriteLine("Source faulted, manually completing target");
        target.Complete(); // Graceful shutdown instead of fault propagation
    }
});
``

## Cancellation vs Faults

**Cancellation** (via `CancellationToken`) is **different** from faults:

``csharp
var cts = new CancellationTokenSource();

var block = new ActionBlock<int>(async x =>
{
    await Task.Delay(1000, cts.Token); // Respects cancellation
    Console.WriteLine(x);
}, new ExecutionDataflowBlockOptions 
{ 
    CancellationToken = cts.Token 
});

block.Post(1);
block.Post(2);

cts.Cancel(); // Request cancellation
block.Complete();

try
{
    await block.Completion;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Block was canceled (not faulted)");
}
``

- **Fault**: Unexpected error (exception)
- **Cancellation**: Intentional, graceful shutdown request

## Best Practices

✅ **Handle exceptions at the right level**
- Item-level: try-catch inside block for recoverable errors
- Pipeline-level: monitor Completion for critical failures

✅ **Log errors explicitly**
``csharp
catch (Exception ex)
{
    logger.LogError(ex, "Processing failed for item {Item}", item);
    return defaultValue; // Continue pipeline
}
``

✅ **Use Result types for error channels**
``csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
}
``

✅ **Set up dead-letter queues**
``csharp
var errorQueue = new BufferBlock<FailedItem>();
// Link failed items to error queue for retry/analysis
``

✅ **Monitor block completion**
``csharp
var allBlocks = new[] { block1, block2, block3 };
var completions = allBlocks.Select(b => b.Completion);

try
{
    await Task.WhenAll(completions);
}
catch (Exception ex)
{
    // At least one block faulted
    foreach (var block in allBlocks)
    {
        if (block.Completion.IsFaulted)
        {
            Console.WriteLine($"Block faulted: {block.Completion.Exception}");
        }
    }
}
``

❌ **Avoid**
- Swallowing exceptions without logging
- Letting faults propagate blindly without monitoring
- Ignoring `Completion` task status
- Throwing exceptions for expected validation failures (use filtering instead)

## When to Use Each Strategy

| Scenario | Strategy |
|----------|----------|
| Expected validation errors | Filtering with predicates |
| Transient failures (network) | Try-catch with retry logic |
| Critical data corruption | Let fault propagate, stop pipeline |
| Bulk import with some bad data | Item-level try-catch, continue processing |
| User input validation | Return error results, don't throw |

## Quick Reference

| Concept | Description |
|---------|-------------|
| `block.Completion` | Task representing block's final state |
| `PropagateCompletion = true` | Propagates faults downstream |
| `AggregateException` | Wrapper for block exceptions |
| Try-catch inside block | Item-level error handling |
| Link predicates | Route errors to separate path |
| `CancellationToken` | Graceful shutdown (not a fault) |
