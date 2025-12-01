# Lab 09 Reference Guide

## Hints

### TODO N1: Thread-Safe Progress Tracking

**Hint 1**: Declare three `int` fields at class level:
```csharp
private int _processedCount = 0;
private int _skippedCount = 0;
private int _failedCount = 0;
```

**Hint 2**: Use `Interlocked.Increment(ref fieldName)` to atomically increment counters. Example:
```csharp
Interlocked.Increment(ref _processedCount);
```

**Hint 3**: Check `_cts.Token.IsCancellationRequested` at the start of `ProcessFileAsync`:
```csharp
if (_cts.Token.IsCancellationRequested)
{
    Interlocked.Increment(ref _skippedCount);
    return;
}
```

**Hint 4**: Wrap the processing logic in try-catch to handle `OperationCanceledException` and other exceptions.

---

### TODO N2: Graceful Shutdown with Timeout

**Hint 1**: Call `_block.Complete()` first to signal no more items will be posted.

**Hint 2**: Use `WaitAsync` extension method with timeout:
```csharp
await _block.Completion.WaitAsync(TimeSpan.FromSeconds(5));
```

**Hint 3**: Wrap in try-catch to handle both `TimeoutException` and `OperationCanceledException`.

**Hint 4**: In the timeout catch block, call `_cts.Cancel()` and then await `_block.Completion` again.

**Hint 5**: Return `true` for graceful completion, `false` for forced cancellation.

---

### TODO N3: Pass CancellationToken to Async Operations

**Hint 1**: Pass `_cts.Token` as the second parameter to `File.ReadAllTextAsync`:
```csharp
var content = await File.ReadAllTextAsync(filePath, _cts.Token);
```

**Hint 2**: Pass `_cts.Token` to `Task.Delay`:
```csharp
await Task.Delay(50, _cts.Token);
```

**Hint 3**: Set `CancellationToken` in the block options:
```csharp
new ExecutionDataflowBlockOptions
{
    MaxDegreeOfParallelism = 3,
    CancellationToken = _cts.Token
}
```

---

## Common Mistakes

1. **Using `++` instead of `Interlocked.Increment`** - This causes race conditions with parallel processing
2. **Not catching `OperationCanceledException` in `ShutdownAsync`** - The block completion can throw when canceled
3. **Forgetting to pass token to BOTH block options AND async operations** - Both are needed for full cancellation support
4. **Not checking `IsCancellationRequested` before processing** - This prevents counting items that should be skipped

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter09;

public record ProcessingStats(int Processed, int Skipped, int Failed, int Total);

public class FileProcessor
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ActionBlock<string> _block;
    
    private int _processedCount = 0;
    private int _skippedCount = 0;
    private int _failedCount = 0;
    
    public FileProcessor()
    {
        _block = new ActionBlock<string>(
            ProcessFileAsync,
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 3,
                CancellationToken = _cts.Token
            });
    }
    
    private async Task ProcessFileAsync(string filePath)
    {
        try
        {
            if (_cts.Token.IsCancellationRequested)
            {
                Interlocked.Increment(ref _skippedCount);
                return;
            }
            
            var content = await File.ReadAllTextAsync(filePath, _cts.Token);
            
            // Simulate processing
            await Task.Delay(50, _cts.Token);
            
            // Count words
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            Interlocked.Increment(ref _processedCount);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref _skippedCount);
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _failedCount);
        }
    }
    
    public bool Post(string filePath) => _block.Post(filePath);
    
    public void Cancel() => _cts.Cancel();
    
    public async Task<bool> ShutdownAsync()
    {
        _block.Complete();
        
        try
        {
            await _block.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            return true; // Graceful shutdown
        }
        catch (OperationCanceledException)
        {
            // Block was already canceled before we called Complete()
            return false;
        }
        catch (TimeoutException)
        {
            _cts.Cancel();
            
            try
            {
                await _block.Completion;
            }
            catch (OperationCanceledException)
            {
                // Expected during forced cancellation
            }
            
            return false; // Forced cancellation
        }
    }
    
    public ProcessingStats GetStats()
    {
        return new ProcessingStats(
            _processedCount,
            _skippedCount,
            _failedCount,
            _processedCount + _skippedCount + _failedCount
        );
    }
}
```

</details>
