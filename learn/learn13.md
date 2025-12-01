# Learn 13: IAsyncEnumerable & Async Streams

## Introduction

**IAsyncEnumerable<T>** brings the power of `async`/`await` to collection iteration. Introduced in C# 8.0, it enables you to consume sequences of data that arrive asynchronously—perfect for streaming APIs, database result sets, real-time sensors, or paginated HTTP requests—without blocking threads or loading everything into memory at once.

### Why Async Streams?

**Problem with traditional approaches:**

```csharp
// ❌ Blocks thread while waiting for each item
IEnumerable<string> GetLogs() {
    foreach (var line in ReadFromSlowSource()) {
        yield return line; // blocks here
    }
}

// ❌ Loads everything into memory first
async Task<List<string>> GetLogsAsync() {
    var list = new List<string>();
    while (await reader.ReadLineAsync() is string line) {
        list.Add(line);
    }
    return list; // all items in memory
}
```

**Solution with async streams:**

```csharp
// ✅ Asynchronous, lazy, memory-efficient
async IAsyncEnumerable<string> GetLogsAsync() {
    while (await reader.ReadLineAsync() is string line) {
        yield return line; // async + lazy!
    }
}

// Consume with await foreach
await foreach (var log in GetLogsAsync()) {
    Console.WriteLine(log);
}
```

### Key Benefits

1. **Non-blocking** - Frees threads while waiting for data
2. **Lazy evaluation** - Items produced on-demand
3. **Memory efficient** - No need to buffer entire sequence
4. **Composable** - Works seamlessly with LINQ and async patterns

---

## Core Concepts

### 1. IAsyncEnumerable<T> Interface

```csharp
public interface IAsyncEnumerable<out T>
{
    IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default);
}

public interface IAsyncEnumerator<out T> : IAsyncDisposable
{
    ValueTask<bool> MoveNextAsync();
    T Current { get; }
}
```

**Key differences from IEnumerable<T>:**

| Aspect | IEnumerable<T> | IAsyncEnumerable<T> |
|--------|----------------|---------------------|
| MoveNext | `bool MoveNext()` | `ValueTask<bool> MoveNextAsync()` |
| Disposal | `IDisposable` | `IAsyncDisposable` |
| Iteration | `foreach` | `await foreach` |
| Producer | `yield return` | `async` + `yield return` |

---

### 2. Producing Async Streams

**Basic producer pattern:**

```csharp
public async IAsyncEnumerable<int> GenerateNumbersAsync(int count)
{
    for (int i = 0; i < count; i++)
    {
        await Task.Delay(100); // Simulate async work
        yield return i;
    }
}
```

**With cancellation support:**

```csharp
public async IAsyncEnumerable<string> FetchPagesAsync(
    string url,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    int page = 1;
    while (!cancellationToken.IsCancellationRequested)
    {
        var data = await httpClient.GetStringAsync($"{url}?page={page}", cancellationToken);
        yield return data;
        page++;
        
        if (string.IsNullOrEmpty(data)) break;
    }
}
```

**Key points:**
- Method signature: `async IAsyncEnumerable<T>`
- Use `yield return` for items (like IEnumerable)
- Use `await` for async operations
- `[EnumeratorCancellation]` passes cancellation through `await foreach`

---

### 3. Consuming Async Streams

**Basic consumption:**

```csharp
await foreach (var item in GetItemsAsync())
{
    Console.WriteLine(item);
}
```

**With cancellation:**

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

await foreach (var item in GetItemsAsync().WithCancellation(cts.Token))
{
    Console.WriteLine(item);
}
```

**Manual enumeration:**

```csharp
await using var enumerator = stream.GetAsyncEnumerator();
while (await enumerator.MoveNextAsync())
{
    var item = enumerator.Current;
    // Process item
}
```

---

## Common Patterns

### Pattern 1: Database Result Streaming

**Problem:** Loading millions of rows exhausts memory.

**Solution:** Stream results one-by-one.

```csharp
public async IAsyncEnumerable<Customer> GetCustomersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync(ct);
    
    await using var command = new SqlCommand("SELECT * FROM Customers", connection);
    await using var reader = await command.ExecuteReaderAsync(ct);
    
    while (await reader.ReadAsync(ct))
    {
        yield return new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        };
    }
}

// Usage
await foreach (var customer in GetCustomersAsync())
{
    await ProcessCustomerAsync(customer);
}
```

---

### Pattern 2: Paginated API Calls

```csharp
public async IAsyncEnumerable<Product> FetchAllProductsAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    string? nextPageToken = null;
    
    do
    {
        var response = await httpClient.GetFromJsonAsync<PagedResponse>(
            $"/api/products?pageToken={nextPageToken}", ct);
        
        foreach (var product in response.Items)
        {
            yield return product;
        }
        
        nextPageToken = response.NextPageToken;
    }
    while (nextPageToken != null);
}
```

---

### Pattern 3: Real-Time Data Streams

**Sensor readings, stock prices, log tailing:**

```csharp
public async IAsyncEnumerable<SensorReading> MonitorSensorAsync(
    string sensorId,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    while (!ct.IsCancellationRequested)
    {
        var reading = await sensorClient.GetReadingAsync(sensorId, ct);
        yield return reading;
        
        await Task.Delay(TimeSpan.FromSeconds(1), ct);
    }
}

// Usage with timeout
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
await foreach (var reading in MonitorSensorAsync("temp-01", cts.Token))
{
    Console.WriteLine($"Temp: {reading.Value}°C");
}
```

---

### Pattern 4: Transforming Async Streams

**Manual transformation:**

```csharp
public async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(
    this IAsyncEnumerable<TIn> source,
    Func<TIn, TOut> selector)
{
    await foreach (var item in source)
    {
        yield return selector(item);
    }
}

// Usage
await foreach (var upper in GetNamesAsync().SelectAsync(n => n.ToUpper()))
{
    Console.WriteLine(upper);
}
```

**Using System.Linq.Async (NuGet package):**

```csharp
using System.Linq;

var numbers = GenerateNumbersAsync(100)
    .Where(n => n % 2 == 0)
    .Select(n => n * n)
    .Take(10);

await foreach (var square in numbers)
{
    Console.WriteLine(square);
}
```

---

### Pattern 5: Buffering & Batching

**Collect items into batches:**

```csharp
public static async IAsyncEnumerable<T[]> BufferAsync<T>(
    this IAsyncEnumerable<T> source,
    int batchSize)
{
    var buffer = new List<T>(batchSize);
    
    await foreach (var item in source)
    {
        buffer.Add(item);
        
        if (buffer.Count >= batchSize)
        {
            yield return buffer.ToArray();
            buffer.Clear();
        }
    }
    
    if (buffer.Count > 0)
    {
        yield return buffer.ToArray();
    }
}

// Usage
await foreach (var batch in GetLogsAsync().BufferAsync(100))
{
    await BulkInsertAsync(batch);
}
```

---

## Integration with Other Patterns

### With TPL Dataflow

**Stream items into a dataflow block:**

```csharp
var buffer = new BufferBlock<string>();

// Producer
_ = Task.Run(async () =>
{
    await foreach (var item in GetItemsAsync())
    {
        await buffer.SendAsync(item);
    }
    buffer.Complete();
});

// Consumer
while (await buffer.OutputAvailableAsync())
{
    var item = await buffer.ReceiveAsync();
    Console.WriteLine(item);
}
```

**Create async stream from dataflow:**

```csharp
public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
    this ISourceBlock<T> source,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    while (await source.OutputAvailableAsync(ct))
    {
        while (source.TryReceive(out var item))
        {
            yield return item;
        }
    }
}
```

---

### With Reactive Extensions (Rx.NET)

**IAsyncEnumerable → IObservable:**

```csharp
public static IObservable<T> ToObservable<T>(this IAsyncEnumerable<T> source)
{
    return Observable.Create<T>(async (observer, ct) =>
    {
        try
        {
            await foreach (var item in source.WithCancellation(ct))
            {
                observer.OnNext(item);
            }
            observer.OnCompleted();
        }
        catch (Exception ex)
        {
            observer.OnError(ex);
        }
    });
}
```

**IObservable → IAsyncEnumerable:**

```csharp
public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IObservable<T> source)
{
    return AsyncEnumerable.Create<T>(async (yield, ct) =>
    {
        var tcs = new TaskCompletionSource<bool>();
        
        using var subscription = source.Subscribe(
            onNext: async item => await yield.ReturnAsync(item),
            onError: ex => tcs.SetException(ex),
            onCompleted: () => tcs.SetResult(true)
        );
        
        await tcs.Task;
    });
}
```

---

### With Channels

**Channel → IAsyncEnumerable:**

```csharp
public static async IAsyncEnumerable<T> ReadAllAsync<T>(
    this ChannelReader<T> reader,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    while (await reader.WaitToReadAsync(ct))
    {
        while (reader.TryRead(out var item))
        {
            yield return item;
        }
    }
}

// Usage
var channel = Channel.CreateUnbounded<int>();
await foreach (var item in channel.Reader.ReadAllAsync())
{
    Console.WriteLine(item);
}
```

---

## Cancellation & Disposal

### Cancellation Token Propagation

```csharp
// Producer with [EnumeratorCancellation]
public async IAsyncEnumerable<int> GetDataAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    for (int i = 0; i < 100; i++)
    {
        ct.ThrowIfCancellationRequested();
        await Task.Delay(100, ct);
        yield return i;
    }
}

// Consumer passes token
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
await foreach (var item in GetDataAsync().WithCancellation(cts.Token))
{
    Console.WriteLine(item);
}
```

**Why `[EnumeratorCancellation]`?**
- Allows token to be passed implicitly via `WithCancellation()`
- Compiler merges consumer token with any producer-level token
- Enables proper cancellation throughout the chain

---

### Async Disposal

```csharp
public async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
{
    await using var stream = File.OpenRead(filePath);
    using var reader = new StreamReader(stream);
    
    while (await reader.ReadLineAsync() is string line)
    {
        yield return line;
    }
    // stream.DisposeAsync() called automatically
}
```

**Manual disposal:**

```csharp
await using var enumerator = stream.GetAsyncEnumerator();
try
{
    while (await enumerator.MoveNextAsync())
    {
        // Process
    }
}
finally
{
    // DisposeAsync called here
}
```

---

## Performance Considerations

### Memory Efficiency

**Before (loads all into memory):**

```csharp
var items = await GetAllItemsAsync(); // List<T> in memory
foreach (var item in items) { /*...*/ }
```

**After (streams one at a time):**

```csharp
await foreach (var item in GetItemsAsync()) { /*...*/ }
```

**Benchmark example:**
- **IEnumerable approach:** 500 MB memory for 1M items
- **IAsyncEnumerable approach:** ~10 MB memory (constant)

---

### ConfigureAwait(false)

**For library code:**

```csharp
public async IAsyncEnumerable<T> GetItemsAsync()
{
    await foreach (var item in source.ConfigureAwait(false))
    {
        yield return item;
    }
}
```

---

### ValueTask vs Task

`IAsyncEnumerator<T>` uses `ValueTask<bool>` for `MoveNextAsync()`:
- **Avoids allocation** when result is synchronously available
- **Hot path optimization** for high-frequency iteration
- **Same semantics** as `Task<bool>` but more efficient

---

## Common Pitfalls

### ❌ Mistake 1: Buffering Entire Stream

```csharp
// ❌ Defeats the purpose of streaming
var list = new List<int>();
await foreach (var item in GetItemsAsync())
{
    list.Add(item);
}
return list; // All in memory
```

**✅ Better:**

```csharp
await foreach (var item in GetItemsAsync())
{
    await ProcessItemAsync(item); // Process immediately
}
```

---

### ❌ Mistake 2: Not Handling Exceptions

```csharp
// ❌ Exception aborts iteration silently
await foreach (var item in GetItemsAsync())
{
    DoWork(item); // May throw
}
```

**✅ Better:**

```csharp
await foreach (var item in GetItemsAsync())
{
    try
    {
        DoWork(item);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process item");
        // Decide: continue, break, or rethrow
    }
}
```

---

### ❌ Mistake 3: Multiple Enumeration

```csharp
var stream = GetItemsAsync();

// ❌ First enumeration
await foreach (var item in stream) { /*...*/ }

// ❌ Second enumeration - may fail or repeat work
await foreach (var item in stream) { /*...*/ }
```

**✅ Solution:** Cache if needed, or redesign to enumerate once.

```csharp
var items = await stream.ToListAsync(); // System.Linq.Async
// Now safe to enumerate multiple times
```

---

### ❌ Mistake 4: Forgetting Cancellation

```csharp
// ❌ No way to stop infinite stream
public async IAsyncEnumerable<int> GetInfiniteAsync()
{
    int i = 0;
    while (true)
    {
        await Task.Delay(1000);
        yield return i++;
    }
}
```

**✅ Better:**

```csharp
public async IAsyncEnumerable<int> GetInfiniteAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    int i = 0;
    while (!ct.IsCancellationRequested)
    {
        await Task.Delay(1000, ct);
        yield return i++;
    }
}
```

---

## Testing Strategies

### Unit Testing Producers

```csharp
[Fact]
public async Task Producer_YieldsExpectedSequence()
{
    // Arrange
    var expected = new[] { 1, 2, 3 };
    
    // Act
    var actual = new List<int>();
    await foreach (var item in ProduceNumbersAsync(3))
    {
        actual.Add(item);
    }
    
    // Assert
    Assert.Equal(expected, actual);
}
```

---

### Testing with Cancellation

```csharp
[Fact]
public async Task Producer_RespectsCancellation()
{
    // Arrange
    var cts = new CancellationTokenSource();
    var count = 0;
    
    // Act
    var task = Task.Run(async () =>
    {
        await foreach (var item in GetInfiniteAsync(cts.Token))
        {
            count++;
            if (count == 5) cts.Cancel();
        }
    });
    
    // Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() => task);
    Assert.Equal(5, count);
}
```

---

### Mock Async Streams

```csharp
public static async IAsyncEnumerable<T> FromArray<T>(params T[] items)
{
    foreach (var item in items)
    {
        await Task.Yield(); // Simulate async
        yield return item;
    }
}

// Test usage
await foreach (var item in FromArray(1, 2, 3))
{
    Console.WriteLine(item);
}
```

---

## Comparison: When to Use What?

| Scenario | Use | Rationale |
|----------|-----|-----------|
| **Large dataset from DB** | IAsyncEnumerable | Memory efficient, lazy loading |
| **Event stream (UI, sensors)** | IObservable (Rx) | Time-based operators, hot streams |
| **Producer-consumer queue** | Channels | High-performance, bounded buffering |
| **ETL pipeline** | TPL Dataflow | Complex transformations, parallelism |
| **Paginated API** | IAsyncEnumerable | Natural async iteration |
| **File line-by-line** | IAsyncEnumerable | Lazy, memory-safe |
| **Real-time dashboard** | IObservable | Reactive UI updates |
| **Background job queue** | Channels | Work distribution, backpressure |

---

## Best Practices

### ✅ Do

1. **Use `[EnumeratorCancellation]`** for proper cancellation support
2. **Implement IAsyncDisposable** when holding resources
3. **ConfigureAwait(false)** in library code
4. **Process items immediately** (avoid buffering entire stream)
5. **Add timeout/cancellation** to long-running streams
6. **Use System.Linq.Async** for composition
7. **Test cancellation behavior** explicitly

### ❌ Don't

1. **Don't buffer entire stream** unless necessary
2. **Don't enumerate multiple times** without caching
3. **Don't ignore exceptions** in producers
4. **Don't forget disposal** (use `await using`)
5. **Don't block threads** (`Task.Result`, `Wait()`)
6. **Don't mix sync/async** enumeration patterns
7. **Don't over-complicate** simple scenarios

---

## Exercise Challenge

**Build a Log Aggregator:**

Create an async stream that:
1. Monitors multiple log files simultaneously
2. Merges log entries by timestamp
3. Filters by log level (Info, Warning, Error)
4. Supports cancellation
5. Processes entries as they arrive (no buffering)

**Bonus:** Add rate limiting (max N entries per second)

---

## Additional Resources

### Official Documentation
- [Async Streams (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-streams)
- [IAsyncEnumerable<T> Interface](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1)

### Articles & Tutorials
- [Async Streams in C# — Real-Time Data Processing](https://metadesignsolutions.com/async-streams-in-c-real-time-data-processing-in-net/)
- [Working with IAsyncEnumerable - Red Gate](https://www.red-gate.com/simple-talk/development/dotnet-development/working-with-iasyncenumerable-in-c/)
- [Efficient Data Handling with Chunking](https://skerdiberberi.com/blog/iasyncenumerable)

### Libraries
- **System.Linq.Async** - LINQ operators for async streams
- **AsyncEnumerator** - Helper utilities
- **Channels** - For integration patterns

---

## Summary

**IAsyncEnumerable<T>** brings async/await elegance to sequence iteration:

✅ **Non-blocking** - Async operations don't block threads  
✅ **Lazy** - Items produced on-demand  
✅ **Memory efficient** - Stream large datasets  
✅ **Cancellable** - Proper cancellation support  
✅ **Composable** - Works with LINQ, Dataflow, Rx, Channels  

**Key syntax:**
- Producer: `async IAsyncEnumerable<T>` + `yield return` + `await`
- Consumer: `await foreach`
- Cancellation: `[EnumeratorCancellation]` + `WithCancellation()`
- Disposal: `await using`

**Perfect for:** Database streaming, paginated APIs, real-time data, file processing, and any scenario where data arrives asynchronously over time.

Next: Apply these concepts in Quiz 13! 🚀
