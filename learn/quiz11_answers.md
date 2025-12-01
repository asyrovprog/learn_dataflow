# Quiz 11 Answer Key: Producer-Consumer Patterns Beyond Dataflow

## Q1. Which statement about Channel architecture is TRUE?

**Correct Answer:** A

**Explanation:**
- A) ✅ Correct - Both Writer and Reader are views into the same shared internal queue
- B) ❌ Items are **distributed** (competing), not broadcast. Each item goes to one reader.
- C) ❌ Channels are **thread-safe** internally - no explicit synchronization needed
- D) ❌ Only async methods (`ReadAsync`, `WaitToReadAsync`) - `TryRead` is sync but returns bool

---

## Q2. When using `BoundedChannelFullMode.DropOldest` with capacity=3, what happens when queue `[1,2,3]` receives item `4`?

**Correct Answer:** C

**Explanation:**
- A) ❌ That's `DropWrite` behavior (reject new item)
- B) ❌ Middle item is not removed
- C) ✅ **Item 1 (oldest, at front)** is removed, new item added to end → `[2,3,4]` (ring buffer)
- D) ❌ Newest item in queue is not removed

**Pattern:** Classic ring buffer - removes from front (oldest), adds to back (newest).

---

## Q3. What is the PRIMARY difference between Channels and TPL Dataflow?

**Correct Answer:** C

**Explanation:**
- A) ❌ Both support backpressure
- B) ❌ **Channels** are faster (10x), not Dataflow
- C) ✅ Channels = storage queue; Dataflow = storage + processing pipeline
- D) ❌ Dataflow provides transformation blocks, not Channels

---

## Q4. Which reading pattern is BEST for consuming all items until channel completes?

**Correct Answer:** C

**Explanation:**
- A) ❌ Doesn't wait for items or handle completion
- B) ❌ Reads single item, doesn't loop until completion
- C) ✅ **Best practice** - idiomatic C# 8.0+, handles completion automatically
- D) ❌ Works but more verbose than `ReadAllAsync()`

---

## Q5. What happens if you NEVER call `channel.Writer.Complete()`?

**Correct Answer:** ACD

**Explanation:**
- A) ✅ `ReadAllAsync()` waits for completion signal that never comes
- B) ❌ No automatic timeout - hangs forever
- C) ✅ `WaitToReadAsync()` keeps returning true, never signals "done"
- D) ✅ Unbounded channel grows indefinitely = memory leak

---

## Q6. When should you use `TryWrite()` instead of `WriteAsync()`?

**Correct Answer:** ABC

**Explanation:**
- A) ✅ `TryWrite()` returns immediately (never blocks)
- B) ✅ Returns false if full, lets you handle gracefully
- C) ✅ Synchronous = no Task allocation overhead
- D) ❌ `TryWrite()` can **fail** (returns false) - use `WriteAsync()` for guaranteed delivery

---

## Q7. Benchmark shows Channels are ~10x faster than BufferBlock for simple producer-consumer. Why?

**Correct Answer:** B

**Explanation:**
- A) ❌ Both use efficient synchronization (not main reason)
- B) ✅ Channels = **lightweight storage**; Dataflow = storage + message passing + completion propagation (more overhead)
- C) ❌ Memory allocation not the primary difference
- D) ❌ Both use ThreadPool

---

## Q8. What optimization does `SingleReader = true` provide?

**Correct Answer:** B

**Explanation:**
- A) ❌ Doesn't **prevent** multiple readers (just optimizes for single reader case)
- B) ✅ Channel can use **faster lock-free algorithms** when only one reader
- C) ❌ No automatic batching
- D) ❌ Doesn't prevent calls, just optimizes internal implementation

---

## Q9. In a multi-producer scenario with 3 producers, how do you coordinate channel completion?

**Correct Answer:** B

**Explanation:**
- A) ❌ Calling `Complete()` multiple times causes exceptions
- B) ✅ **Pattern:** `if (Interlocked.Decrement(ref count) == 0) Complete();`
- C) ❌ Can't complete before producers start (no items would be written)
- D) ❌ No automatic completion - must call `Complete()` explicitly

---

## Q10. Why use bounded channels over unbounded?

**Correct Answer:** AC

**Explanation:**
- A) ✅ Bounded capacity = automatic backpressure when full
- B) ❌ Unbounded can be **faster** (no blocking), but risks memory issues
- C) ✅ Blocking/dropping behavior signals consumer slowness
- D) ❌ Both support completion via `Complete()`

---

## Scoring

- **9-10 correct:** Excellent! Ready for Channels in production
- **7-8 correct:** Good understanding, review completion patterns
- **5-6 correct:** Revisit bounded vs unbounded behavior
- **<5 correct:** Review learning materials, focus on core concepts
