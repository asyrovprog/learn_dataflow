# Quiz 09 Retry Answer Key: CancellationToken Integration & Graceful Shutdown

---

## Q1: Token Passing

**Correct Answer**: Y

**Reasoning**: You must pass the token to BOTH locations: `DataflowBlockOptions.CancellationToken` (so the block infrastructure respects cancellation) AND to async operations inside delegates (like `HttpClient.GetAsync(url, token)`) so those operations can be canceled mid-execution.

---

## Q2: Block State After Cancel

**Correct Answer**: B

**Reasoning**: Cancellation signals the block to stop accepting NEW items (`Post()` returns false), but items already in the input buffer are still processed. This is cooperative cancellation - queued work completes before the block transitions to Canceled state.

---

## Q3: Linked Token Use Case

**Correct Answer**: ABC

**Reasoning**: The pattern is: (A) create a parent CTS, (B) create linked tokens for each child from the parent, and (C) monitor each child and cancel the parent on failure. Using a shared CTS directly (D) works but doesn't give you per-child control for cancellation tracking.

---

## Q4: Completion Timeout Pattern

**Correct Answer**: B

**Reasoning**: Graceful shutdown follows: Complete() → WaitAsync(timeout) → if timeout, Cancel() → await Completion again → Dispose. This gives queued work a chance to finish before forcing cancellation.

---

## Q5: OCE in Delegates

**Correct Answer**: N

**Reasoning**: If you catch `OperationCanceledException` inside the delegate and return a normal value, the block treats it as successful processing. The block only faults if the exception ESCAPES the delegate.

---

## Q6: CTS Disposal Timing

**Correct Answer**: C

**Reasoning**: Dispose the CTS only AFTER all blocks have completed (awaited their `Completion` tasks). Blocks may still be checking `IsCancellationRequested` during shutdown, and disposing while they're active can cause `ObjectDisposedException`.

---

## Q7: Partial Results Pattern

**Correct Answer**: ABCD

**Reasoning**: All four are valid patterns: (A) thread-safe counters track progress, (B) continuations log final stats, (C) checking the token lets you stop expensive work early, (D) storing results as they complete preserves partial output for reporting.

---

## Q8: Post() After Cancellation

**Correct Answer**: Y

**Reasoning**: Once a block is canceled, `Post()` returns `false` because the block stops accepting new items. This is how you detect that cancellation has taken effect.

---

## Score

All answers correct → **8/8 (100%)**.
