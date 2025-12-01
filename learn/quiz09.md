# Quiz 09 Retry: CancellationToken Integration & Graceful Shutdown

Fresh set of questions to reinforce your understanding of cancellation patterns in TPL Dataflow.

**Format**:

- Y/N questions: Answer with Y or N
- Multiple choice (A-D): Answer with the letter
- Multi-select (M:): Answer with multiple letters like "AC" or "BD"

---

## Q1: Token Passing (Y/N)

Must you pass a `CancellationToken` to both `DataflowBlockOptions.CancellationToken` AND to any async operations called inside the block delegate for proper cancellation support?

**Answer**:

---

## Q2: Block State After Cancel (A-D)

After calling `cts.Cancel()` on a token passed to a block's options, what happens to items already queued in the block's input buffer?

A) They are immediately discarded

B) They are still processed before the block completes

C) They throw `OperationCanceledException` automatically

D) They remain queued forever, blocking completion

**Answer**:

---

## Q3: Linked Token Use Case (M:)

You have three independent child pipelines (payments, inventory, shipping) and want any failure in one to stop all others. Which strategy should you use? (Select all that apply)

A) Create a parent `CancellationTokenSource`

B) Create linked tokens from the parent for each child pipeline

C) Monitor each child's `Completion` task and call `parentCts.Cancel()` on any failure

D) Use a shared `CancellationTokenSource` directly in all three pipelines

**Answer**:

---

## Q4: Completion Timeout Pattern (A-D)

What is the correct sequence for graceful shutdown with timeout?

A) Cancel → Complete → Wait → Dispose

B) Complete → Wait with timeout → Cancel if timeout → Wait again → Dispose

C) Wait → Cancel → Complete → Dispose

D) Dispose → Cancel → Complete

**Answer**:

---

## Q5: OCE in Delegates (Y/N)

If you catch `OperationCanceledException` inside a block delegate and return a default value instead of rethrowing, will the block still fault?

**Answer**:

---

## Q6: CTS Disposal Timing (A-D)

When is it safe to dispose a `CancellationTokenSource` that was passed to block options?

A) Immediately after calling `Cancel()`

B) After calling `Complete()` on all blocks

C) After awaiting all block `Completion` tasks

D) Before starting the pipeline to avoid memory leaks

**Answer**:

---

## Q7: Partial Results Pattern (M:)

Which patterns help track partial progress during cancellation? (Select all that apply)

A) Use `Interlocked` operations to track processed/skipped counts

B) Attach a continuation to `Completion` that logs statistics

C) Check `cts.IsCancellationRequested` periodically in delegates

D) Store results in a thread-safe collection as items complete

**Answer**:

---

## Q8: Post() After Cancellation (Y/N)

After cancellation is triggered, will calling `Post()` on a canceled block return `false`?

**Answer**:

---

## Scoring

- 8/8 correct: 100% ✅ Perfect!
- 6-7 correct: 75-88% ✅ Solid Understanding
- <6 correct: <75% ⚠️ Revisit the lesson
