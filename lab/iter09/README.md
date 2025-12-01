# Lab 09: Cancellable File Processor

Build a dataflow pipeline that processes text files with support for graceful shutdown, timeout-based cancellation, and partial progress tracking.

---

## Scenario

You're building a document analysis tool that:
- Processes text files to count words and extract keywords
- Supports user cancellation during processing
- Implements graceful shutdown with 5-second timeout
- Tracks how many files were processed vs skipped during cancellation
- Uses thread-safe counters for accurate statistics

---

## TODO N1 – Implement Thread-Safe Progress Tracking

**Objective**: Add thread-safe counters using `Interlocked` operations to track processed, skipped, and failed files.

**Requirements**:
1. Add three `int` fields: `_processedCount`, `_skippedCount`, `_failedCount`
2. In `ProcessFileAsync`:
   - Before processing, check if cancellation was requested using `_cts.Token.IsCancellationRequested`
   - If canceled, increment `_skippedCount` using `Interlocked.Increment` and return early
   - On successful processing, increment `_processedCount` using `Interlocked.Increment`
   - On exception (any exception), increment `_failedCount` using `Interlocked.Increment`
3. Implement `GetStats()` to return a `ProcessingStats` record with current counts

**Why Interlocked?**: The pipeline uses `MaxDegreeOfParallelism = 3`, meaning multiple threads update counters simultaneously. Regular `++` operations would cause race conditions and lost counts.

---

## TODO N2 – Implement Graceful Shutdown with Timeout

**Objective**: Implement two-phase shutdown: attempt graceful completion with 5-second timeout, then force cancellation if needed.

**Requirements**:
1. In `ShutdownAsync`:
   - **Phase 1**: Call `_block.Complete()` to stop accepting new files
   - **Phase 2**: Use `await _block.Completion.WaitAsync(TimeSpan.FromSeconds(5))` to wait with timeout
   - **Phase 3**: If `TimeoutException` is thrown:
     - Call `_cts.Cancel()` to force cancellation
     - Await `_block.Completion` again (wrap in try-catch for `OperationCanceledException`)
2. Return `true` if graceful completion succeeded, `false` if forced cancellation was needed

**Two-Phase Pattern**: Give queued files a chance to complete normally before forcing cancellation.

---

## TODO N3 – Pass CancellationToken to Async Operations

**Objective**: Pass the cancellation token to async operations inside the block delegate for responsive cancellation.

**Requirements**:
1. In the `ActionBlock` constructor, pass `_cts.Token` to:
   - `File.ReadAllTextAsync(filePath, _cts.Token)` 
   - `Task.Delay(50, _cts.Token)` (simulates processing time)
2. Also set `CancellationToken = _cts.Token` in `ExecutionDataflowBlockOptions`

**Why Both?**: Block options make the block infrastructure respect cancellation (stop accepting items), while passing token to async operations enables mid-execution cancellation.

---

## Expected Behavior

**Test 1**: Process 3 files without cancellation → all 3 processed, graceful shutdown  
**Test 2**: Cancel immediately → 0 processed, some skipped, forced shutdown  
**Test 3**: Cancel after 100ms → partial processing, accurate counts, forced shutdown  
**Test 4**: Counters are thread-safe even with parallel processing

---

## Files

- `Task.cs` - Your implementation (fill TODOs)
- `Program.cs` - Test harness
- `README.md` - This file
- `REF.md` - Hints and reference solution
