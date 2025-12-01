# Lab 11 - Rate-Limited API Request Queue

## Overview

Build a **rate-limited API request queue** using `System.Threading.Channels` to practice bounded channels, backpressure handling, and rate limiting patterns.

## Scenario

You're building a background service that processes API requests with rate limiting to avoid overwhelming external services. The queue must:
- Accept incoming requests with bounded capacity (backpressure)
- Process requests at a maximum rate (10 requests/second)
- Track success/failure/throttled statistics (thread-safe counters)
- Support graceful cancellation with partial processing

## Architecture

```
┌──────────────┐     Bounded      ┌────────────────────┐
│   Producers  │────Channel(50)───▶│  Consumer Task     │
│ (API calls)  │                   │  + Rate Limiter    │
└──────────────┘                   │  (10 req/sec)      │
                                   └────────────────────┘
                                            │
                                            ▼
                                   ┌────────────────────┐
                                   │   Statistics       │
                                   │ (Interlocked ctrs) │
                                   └────────────────────┘
```

---

## TODO N1 – Bounded Channel Setup & Enqueue Logic

### Goal
Create a bounded channel with proper configuration and implement non-blocking enqueue with TryWrite fallback.

###Instructions
1. **Create bounded channel** in constructor:
   - Use `Channel.CreateBounded<ApiRequest>()`
   - Set capacity from constructor parameter
   - Configure `FullMode = BoundedChannelFullMode.Wait` (block when full)
   - Set `SingleReader = true` (optimization - one consumer)

2. **Implement `EnqueueAsync()` method**:
   - Try non-blocking write first: `if (_channel.Writer.TryWrite(request)) return true;`
   - If queue full, fallback to async: `await _channel.Writer.WriteAsync(request, ct);`
   - Wrap in `try-catch (OperationCanceledException)`
   - On cancellation: increment `_throttledCount` with `Interlocked.Increment()` and return `false`
   - Return `true` on success

### Key Concepts
- **Bounded channels** provide natural backpressure
- **TryWrite** = non-blocking attempt (returns `false` if full)
- **WriteAsync** = async wait for space (respects cancellation)
- **Interlocked** for thread-safe counter updates

---

## TODO N2 – Rate Limiter Initialization

### Goal
Initialize `SemaphoreSlim` to control request processing rate.

### Instructions
1. **Initialize `_rateLimiter`** in constructor:
   - Create `new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond)`
   - Initial count = max count = requests allowed per second
   - Example: `maxRequestsPerSecond = 10` → max 10 concurrent requests

### Key Concepts
- **SemaphoreSlim** controls concurrent access
- **WaitAsync()** acquires slot (blocks if none available)
- **Release()** returns slot after delay (simulates 1-second rate window)

---

## TODO N3 – Consumer with Rate Limiting & Statistics

### Goal
Implement consumer loop that processes requests with rate limiting and tracks statistics.

### Instructions
1. **Consumer loop**:
   - Use `await foreach (var request in _channel.Reader.ReadAllAsync(ct))`
   - Await `_rateLimiter.WaitAsync(ct)` BEFORE processing each request
   
2. **Rate limiting**:
   - After acquiring semaphore, start background task to release after 1 second:
     ```csharp
     _ = Task.Run(async () =>
     {
         await Task.Delay(1000);
         _rateLimiter.Release();
     });
     ```

3. **Process request**:
   - Call `await ProcessRequestAsync(request)`
   - If returns `true`: increment `_successCount` with `Interlocked.Increment(ref _successCount)`
   - If returns `false`: increment `_failedCount` with `Interlocked.Increment(ref _failedCount)`

4. **Cancellation handling**:
   - Wrap entire loop in `try-catch (OperationCanceledException)`
   - Catch block can be empty (expected on shutdown)

### Key Concepts
- **ReadAllAsync()** = idiomatic pattern for consuming until completion
- **Rate limiting** = acquire → process → release after delay
- **Interlocked** = thread-safe counter updates (no locks needed)
- **Fire-and-forget** (`_ = Task.Run`) for delayed Release

---

## Expected Behavior

### Test 1: Bounded Channel
- Enqueue 15 requests to capacity-10 queue
- All 15 processed successfully
- ~13 success, ~2 failed (based on Id % 10 logic)

### Test 2: Rate Limiting
- Process 25 requests at 10 req/sec
- Should take ~2-3 seconds (not instant)

### Test 3: Statistics
- Process 30 requests
- Success: 27, Failed: 3 (every 10th request fails)
- Throttled: 0 (no cancellations)

### Test 4: Graceful Cancellation
- Start processing, cancel after 500ms
- Partial completion (5-9 requests processed)
- No exceptions thrown

---

## Hints

**Hint 1 (TODO N1):** Bounded channels automatically block producers when full if `FullMode = Wait`. TryWrite gives you non-blocking option.

**Hint 2 (TODO N2):** SemaphoreSlim with initial count = max count means "allow N concurrent operations".

**Hint 3 (TODO N3):** Release semaphore AFTER 1-second delay to enforce "10 per second" rate. Don't await the Release task!

**Hint 4 (Interlocked):** Always use `Interlocked.Increment(ref _counter)` when multiple threads update same counter.

---

## Common Mistakes

❌ **Using unbounded channel** → No backpressure, defeats the purpose  
❌ **Forgetting SingleReader optimization** → Slower performance  
❌ **Awaiting Release task** → Blocks processing, breaks rate limiting  
❌ **Using `_counter++`** → Race conditions, incorrect counts  
❌ **Not catching OperationCanceledException** → Unhandled exceptions on shutdown
