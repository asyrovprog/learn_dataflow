# Lab 11 Reference Guide

## Hints

### TODO N1: Bounded Channel & Enqueue

**Hint 1:** Create channel with `BoundedChannelOptions`:
```csharp
_channel = Channel.CreateBounded<ApiRequest>(new BoundedChannelOptions(capacity)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true
});
```

**Hint 2:** TryWrite → WriteAsync pattern:
```csharp
if (_channel.Writer.TryWrite(request))
    return true;  // Fast path: queue not full

// Slow path: wait for space
try
{
    await _channel.Writer.WriteAsync(request, ct);
    return true;
}
catch (OperationCanceledException)
{
    Interlocked.Increment(ref _throttledCount);
    return false;
}
```

---

### TODO N2: Rate Limiter

**Hint 1:** Initialize with same initial and max count:
```csharp
_rateLimiter = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);
```

This creates a semaphore that allows `maxRequestsPerSecond` concurrent acquisitions.

---

### TODO N3: Consumer Loop

**Hint 1:** ReadAllAsync until completion:
```csharp
await foreach (var request in _channel.Reader.ReadAllAsync(ct))
{
    // Process each request
}
```

**Hint 2:** Acquire → Process → Release (delayed):
```csharp
await _rateLimiter.WaitAsync(ct);  // Acquire slot

_ = Task.Run(async () =>           // Fire-and-forget release
{
    await Task.Delay(1000);         // 1-second rate window
    _rateLimiter.Release();
});

// Process request...
```

**Hint 3:** Thread-safe statistics:
```csharp
if (await ProcessRequestAsync(request))
    Interlocked.Increment(ref _successCount);
else
    Interlocked.Increment(ref _failedCount);
```

**Hint 4:** Handle cancellation:
```csharp
try
{
    await foreach (var request in _channel.Reader.ReadAllAsync(ct))
    {
        // ... processing ...
    }
}
catch (OperationCanceledException)
{
    // Expected on shutdown
}
```

---

## Key Patterns

### Pattern 1: Bounded Channel Backpressure
```csharp
// Channel blocks when full (natural backpressure)
var options = new BoundedChannelOptions(50)
{
    FullMode = BoundedChannelFullMode.Wait  // Block producer
};
```

### Pattern 2: Non-Blocking Write with Fallback
```csharp
// Try fast path first
if (!_channel.Writer.TryWrite(item))
{
    // Fallback to async wait
    await _channel.Writer.WriteAsync(item);
}
```

### Pattern 3: Rate Limiting with SemaphoreSlim
```csharp
await _semaphore.WaitAsync();     // Acquire

_ = Task.Run(async () =>          // Don't await!
{
    await Task.Delay(1000);       // Rate window
    _semaphore.Release();
});

await ProcessAsync();              // Do work
```

### Pattern 4: Thread-Safe Counters
```csharp
// WRONG: Race condition
_counter++;

// CORRECT: Atomic increment
Interlocked.Increment(ref _counter);
```

---

## Common Mistakes

### Mistake 1: Awaiting Release
```csharp
// ❌ WRONG: Blocks processing
await Task.Delay(1000);
_semaphore.Release();

// ✅ CORRECT: Fire-and-forget
_ = Task.Run(async () =>
{
    await Task.Delay(1000);
    _semaphore.Release();
});
```

### Mistake 2: Forgetting Interlocked
```csharp
// ❌ WRONG: Race condition
_successCount++;

// ✅ CORRECT: Thread-safe
Interlocked.Increment(ref _successCount);
```

### Mistake 3: Missing Cancellation Handling
```csharp
// ❌ WRONG: Throws unhandled exception
await foreach (var item in channel.Reader.ReadAllAsync(ct))
{
    // ...
}

// ✅ CORRECT: Catch OCE
try
{
    await foreach (var item in channel.Reader.ReadAllAsync(ct))
    {
        // ...
    }
}
catch (OperationCanceledException)
{
    // Expected
}
```

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lab.Iter11;

public record ApiRequest(int Id, string Endpoint);

public class RateLimitedQueue
{
    private readonly Channel<ApiRequest> _channel;
    private readonly SemaphoreSlim _rateLimiter;
    private int _successCount;
    private int _failedCount;
    private int _throttledCount;

    public RateLimitedQueue(int capacity, int maxRequestsPerSecond)
    {
        // TODO[N1]: Create bounded channel
        _channel = Channel.CreateBounded<ApiRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });
        
        // TODO[N2]: Initialize rate limiter
        _rateLimiter = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);
    }

    public async Task<bool> EnqueueAsync(ApiRequest request, CancellationToken ct = default)
    {
        // TODO[N1]: Enqueue with TryWrite fallback
        if (_channel.Writer.TryWrite(request))
        {
            return true;
        }
        
        try
        {
            await _channel.Writer.WriteAsync(request, ct);
            return true;
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref _throttledCount);
            return false;
        }
    }

    public async Task ProcessRequestsAsync(CancellationToken ct)
    {
        // TODO[N3]: Consumer with rate limiting
        try
        {
            await foreach (var request in _channel.Reader.ReadAllAsync(ct))
            {
                await _rateLimiter.WaitAsync(ct);
                
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    _rateLimiter.Release();
                });
                
                if (await ProcessRequestAsync(request))
                {
                    Interlocked.Increment(ref _successCount);
                }
                else
                {
                    Interlocked.Increment(ref _failedCount);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
    }

    private async Task<bool> ProcessRequestAsync(ApiRequest request)
    {
        await Task.Delay(50);
        return request.Id % 10 != 0;  // Fail every 10th request
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }

    public (int Success, int Failed, int Throttled) GetStatistics()
    {
        return (_successCount, _failedCount, _throttledCount);
    }
}
```

</details>
