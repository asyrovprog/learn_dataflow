# Lab 07 Reference Guide

## TODO N1 Hints – Implement Start Signal Block

### Understanding WriteOnceBlock Construction
WriteOnceBlock accepts a single value and broadcasts it to all consumers. The constructor signature:
```csharp
public WriteOnceBlock<T>(Func<T, T>? cloningFunction)
```

**Key Point**: Always pass `null` for the cloning function because WriteOnceBlock broadcasts the same instance (no cloning).

### CreateStartSignal() Implementation
```csharp
// Create a WriteOnceBlock that holds a boolean signal
_startSignal = new WriteOnceBlock<bool>(null);
```

### BroadcastStartSignal() Implementation
```csharp
// Check if block is created
if (_startSignal == null)
    throw new InvalidOperationException("...");

// Post returns true if accepted, false if rejected
return _startSignal.Post(true);
```

### Key Points
- Use `WriteOnceBlock<bool>` to represent the signal
- Pass `null` for cloning function (always for WriteOnceBlock)
- `Post()` returns `true` on first call, `false` on subsequent calls
- The boolean value `true` represents "start"

---

## TODO N2 Hints – Implement Worker Task Logic

### Worker Flow
1. Print "Waiting for start signal..."
2. Call `ReceiveAsync()` on the start signal block
3. Increment started count
4. Print "Started! Performing work..."
5. Simulate work with `Task.Delay()`
6. Increment completed count
7. Print "Completed!"

### ReceiveAsync Pattern
```csharp
// Wait for the signal (blocks until value is available)
bool signal = await _startSignal.ReceiveAsync();
```

### Thread-Safe Counting
```csharp
// Use Interlocked for thread-safe increments
Interlocked.Increment(ref _startedCount);
```

### Complete Implementation Structure
```csharp
public async Task RunWorkerAsync(int workerId, int workDurationMs)
{
    // 1. Validate
    if (_startSignal == null)
        throw new InvalidOperationException("Start signal not created.");

    // 2. Wait for signal
    Console.WriteLine($"Worker {workerId}: Waiting for start signal...");
    bool signal = await _startSignal.ReceiveAsync();
    
    // 3. Start work
    Interlocked.Increment(ref _startedCount);
    Console.WriteLine($"Worker {workerId}: Started! Performing work...");
    
    // 4. Perform work
    await Task.Delay(workDurationMs);
    
    // 5. Complete
    Interlocked.Increment(ref _completedCount);
    Console.WriteLine($"Worker {workerId}: Completed!");
}
```

### Key Points
- `ReceiveAsync()` waits until a value is posted to the WriteOnceBlock
- Use `Interlocked.Increment()` for thread-safe counter updates
- All workers receive the same signal value from WriteOnceBlock
- Workers start only after signal is broadcast

---

## TODO N3 Hints – Verify Coordination & Completion

### Coordination Pattern
Wait for all worker tasks to complete with a timeout:
```csharp
var allWorkersTask = Task.WhenAll(workerTasks);
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

var completedTask = await Task.WhenAny(allWorkersTask, timeoutTask);
```

### Checking Which Task Completed
```csharp
if (completedTask != allWorkersTask)
    return false; // Timeout occurred
```

### Verifying Counts
```csharp
return _startedCount == expectedWorkerCount && 
       _completedCount == expectedWorkerCount;
```

### Complete Implementation Structure
```csharp
public async Task<bool> WaitForAllWorkersAsync(Task[] workerTasks, int expectedWorkerCount)
{
    // 1. Validate input
    if (workerTasks == null || workerTasks.Length == 0)
        return false;

    // 2. Set up timeout
    var allWorkersTask = Task.WhenAll(workerTasks);
    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
    
    // 3. Race timeout vs completion
    var completedTask = await Task.WhenAny(allWorkersTask, timeoutTask);
    
    // 4. Check if timed out
    if (completedTask != allWorkersTask)
        return false;
    
    // 5. Verify all workers started and completed
    return _startedCount == expectedWorkerCount && 
           _completedCount == expectedWorkerCount;
}
```

### Key Points
- `Task.WhenAll()` creates a task that completes when all tasks complete
- `Task.WhenAny()` returns the first task to complete (timeout or workers)
- Compare the result to determine if timeout occurred
- Verify counts match expected values

---

## Common Patterns

### WriteOnceBlock Signal Broadcasting
```csharp
var signal = new WriteOnceBlock<bool>(null);

// Multiple tasks wait for signal
var tasks = Enumerable.Range(1, 5).Select(i =>
    Task.Run(async () =>
    {
        await signal.ReceiveAsync();
        // Do work...
    }));

// Broadcast signal to all
signal.Post(true);

await Task.WhenAll(tasks);
```

### Timeout Pattern
```csharp
var workTask = DoWorkAsync();
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

var completed = await Task.WhenAny(workTask, timeoutTask);

if (completed == timeoutTask)
{
    // Handle timeout
}
```

---

## Testing Strategy

1. **Create Start Signal**: Call `CreateStartSignal()`
2. **Start Workers**: Launch multiple worker tasks
3. **Broadcast Signal**: Call `BroadcastStartSignal()`
4. **Wait for Completion**: Use `WaitForAllWorkersAsync()`
5. **Verify Counts**: Check started and completed counts
6. **Test Rejection**: Verify second broadcast returns false

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter07
{
    public class WorkflowCoordinator
    {
        private WriteOnceBlock<bool>? _startSignal;
        private int _startedCount = 0;
        private int _completedCount = 0;

        public int StartedCount => _startedCount;
        public int CompletedCount => _completedCount;

        // TODO[N1]: Implement Start Signal Block
        public void CreateStartSignal()
        {
            // Create WriteOnceBlock with null cloning function
            _startSignal = new WriteOnceBlock<bool>(null);
        }

        public bool BroadcastStartSignal()
        {
            if (_startSignal == null)
                throw new InvalidOperationException("Start signal not created. Call CreateStartSignal() first.");
            
            // Post the start signal (true = start)
            return _startSignal.Post(true);
        }

        // TODO[N2]: Implement Worker Task Logic
        public async Task RunWorkerAsync(int workerId, int workDurationMs)
        {
            if (_startSignal == null)
                throw new InvalidOperationException("Start signal not created.");

            Console.WriteLine($"Worker {workerId}: Waiting for start signal...");
            
            // Wait for start signal
            bool signal = await _startSignal.ReceiveAsync();
            
            Interlocked.Increment(ref _startedCount);
            Console.WriteLine($"Worker {workerId}: Started! Performing work...");
            
            // Simulate work
            await Task.Delay(workDurationMs);
            
            Interlocked.Increment(ref _completedCount);
            Console.WriteLine($"Worker {workerId}: Completed!");
        }

        // TODO[N3]: Verify Coordination & Completion
        public async Task<bool> WaitForAllWorkersAsync(Task[] workerTasks, int expectedWorkerCount)
        {
            if (workerTasks == null || workerTasks.Length == 0)
                return false;

            // Wait for all workers with timeout
            var allWorkersTask = Task.WhenAll(workerTasks);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            
            var completedTask = await Task.WhenAny(allWorkersTask, timeoutTask);
            
            // Check if all workers completed (not timeout)
            if (completedTask != allWorkersTask)
                return false;
            
            // Verify all workers started and completed
            return _startedCount == expectedWorkerCount && 
                   _completedCount == expectedWorkerCount;
        }
    }
}
```

</details>
