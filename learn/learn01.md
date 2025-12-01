# Learning C# TPL Dataflow: Foundations and Basics

## What is TPL Dataflow?

TPL Dataflow (Task Parallel Library Dataflow) is a library for building robust, concurrent applications using a **dataflow programming model**. Instead of managing threads directly, you create "blocks" that process data and connect them into pipelines.

**Key Intuition:** Think of it like an assembly line in a factory. Each station (block) does one specific job, and items (data) flow from station to station automatically.

## Core Concepts

### 1. **Dataflow Blocks** - The Building Blocks

There are three main categories:

#### **Source Blocks** (Producers)

- Produce data for downstream consumption
- Example: `BufferBlock<T>` - stores messages and provides them to consumers

#### **Target Blocks** (Consumers)

- Receive and process data
- Example: `ActionBlock<T>` - executes an action for each item

#### **Propagator Blocks** (Transformers)

- Both receive and send data
- Example: `TransformBlock<TInput, TOutput>` - transforms input to output

### 2. **Linking Blocks Together**

Blocks are connected using `LinkTo()` to create pipelines:

```csharp
sourceBlock.LinkTo(targetBlock);
```

### 3. **Completion and Flow Control**

- Blocks can be marked as "complete" (no more data coming)
- Completion propagates through linked blocks
- Built-in buffering and throttling

## Visual Model

```
[Source] --> [Transform] --> [Transform] --> [Action]
  Data       Process        Process        Consume
  Origin     Step 1         Step 2         & Execute
```

## Simple Example: Number Processing Pipeline

```csharp
using System;
using System.Threading.Tasks.Dataflow;

// 1. Create blocks
var multiplyBlock = new TransformBlock<int, int>(n => 
{
    Console.WriteLine($"Multiplying {n} by 2");
    return n * 2;
});

var printBlock = new ActionBlock<int>(n => 
{
    Console.WriteLine($"Result: {n}");
});

// 2. Link them together
multiplyBlock.LinkTo(printBlock, new DataflowLinkOptions 
{ 
    PropagateCompletion = true 
});

// 3. Post data
for (int i = 1; i <= 5; i++)
{
    multiplyBlock.Post(i);
}

// 4. Signal completion and wait
multiplyBlock.Complete();
await printBlock.Completion;
```

**Output:**
```
Multiplying 1 by 2
Result: 2
Multiplying 2 by 2
Result: 4
Multiplying 3 by 2
Result: 6
...
```

## Common Dataflow Blocks Reference

| Block Type | Purpose | Example Use |
|------------|---------|-------------|
| `BufferBlock<T>` | Queue messages | Message queue/buffer |
| `ActionBlock<T>` | Execute action per item | Process/consume data |
| `TransformBlock<TIn,TOut>` | Transform 1:1 | Convert data format |
| `TransformManyBlock<TIn,TOut>` | Transform 1:many | Split one item into multiple |
| `BatchBlock<T>` | Group items into batches | Batch processing |
| `BroadcastBlock<T>` | Copy to all targets | Broadcast same data |

## Key Benefits

1. **Automatic parallelization** - blocks can process items concurrently
2. **Backpressure handling** - built-in flow control
3. **Composability** - easy to build complex pipelines
4. **Separation of concerns** - each block has one job
5. **Async/await friendly** - integrates with modern C# patterns

## Real-World Example: Image Processing Pipeline

```csharp
var downloadBlock = new TransformBlock<string, byte[]>(
    async url => await DownloadImageAsync(url),
    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 }
);

var resizeBlock = new TransformBlock<byte[], byte[]>(
    imageData => ResizeImage(imageData, 800, 600)
);

var saveBlock = new ActionBlock<byte[]>(
    async imageData => await SaveImageAsync(imageData)
);

// Build pipeline
downloadBlock.LinkTo(resizeBlock, new DataflowLinkOptions { PropagateCompletion = true });
resizeBlock.LinkTo(saveBlock, new DataflowLinkOptions { PropagateCompletion = true });

// Process multiple URLs
foreach (var url in imageUrls)
{
    downloadBlock.Post(url);
}

downloadBlock.Complete();
await saveBlock.Completion;
```

## Important Configuration Options

### ExecutionDataflowBlockOptions

- `MaxDegreeOfParallelism` - how many items to process simultaneously
- `BoundedCapacity` - limits buffered items (backpressure)
- `CancellationToken` - cancellation support

### DataflowLinkOptions

- `PropagateCompletion` - auto-complete downstream blocks
- `Predicate` - filter which messages flow through

## When to Use TPL Dataflow

✅ **Good for:**

- ETL pipelines (Extract, Transform, Load)
- Data processing workflows
- Producer-consumer patterns
- Complex concurrent operations
- Message processing systems

❌ **Not ideal for:**

- Simple single-step operations (use `Task` instead)
- Very low-latency requirements
- When you need fine-grained thread control

## Quick Start Checklist

1. ✅ Install package: `System.Threading.Tasks.Dataflow`
2. ✅ Identify your pipeline stages (what operations?)
3. ✅ Choose appropriate block types
4. ✅ Link blocks with `LinkTo()`
5. ✅ Configure parallelism and capacity
6. ✅ Post data and signal completion
7. ✅ Await final block completion
