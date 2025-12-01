# Quiz 07 Answer Key: WriteOnceBlock & Immutable Broadcasting

---

## Q1: Single-Assignment Semantics
**Correct Answer**: Y

**Reasoning**: WriteOnceBlock uses first-wins semantics. When multiple producers post simultaneously, exactly one Post() will return true (accepting the value), and all others will return false (rejecting their values). This is the core single-assignment guarantee.

---

## Q2: Cloning Behavior
**Correct Answer**: B

**Reasoning**: The cloning function parameter should always be `null` for WriteOnceBlock because it never clones messages—it broadcasts the same instance to all targets. This is a key difference from BroadcastBlock, which requires a cloning function.

---

## Q3: Post Return Value
**Correct Answer**: C

**Reasoning**: Once a WriteOnceBlock has accepted a value, all subsequent `Post()` calls return `false`, indicating the value was rejected. The block does not throw exceptions or overwrite—it simply rejects new values gracefully.

---

## Q4: Broadcasting vs Latest-Wins
**Correct Answer**: B

**Reasoning**: WriteOnceBlock accepts exactly one value (first-wins), while BroadcastBlock accepts unlimited values and always keeps the latest. This is the fundamental behavioral difference between the two blocks.

---

## Q5: Use Case Selection
**Correct Answer**: ACE

**Reasoning**:
- **A) ✓** Racing cache lookups is a perfect first-wins scenario
- **B) ✗** Real-time updates need BroadcastBlock (continuous values)
- **C) ✓** Single initialization is a classic WriteOnceBlock pattern
- **D) ✗** Streaming data requires accepting multiple values
- **E) ✓** Broadcasting a single start signal fits perfectly

WriteOnceBlock is for single-value scenarios, not streaming/continuous data.

---

## Q6: Completion Behavior
**Correct Answer**: C

**Reasoning**: Calling `ReceiveAsync()` on a completed WriteOnceBlock that has no value throws `InvalidOperationException`. This is why best practice recommends using `TryReceive()` when you're unsure if a value is available, as it returns false instead of throwing.

---

## Q7: Instance Sharing
**Correct Answer**: Y

**Reasoning**: Yes, WriteOnceBlock broadcasts the exact same object instance to all targets without cloning. This is the "immutable broadcasting" behavior that makes it suitable for sharing immutable objects efficiently across multiple consumers.

---

## Q8: Race Condition Pattern
**Correct Answer**: B

**Reasoning**: In lazy initialization, multiple threads may attempt to post the initialized resource. The first `Post()` that executes returns true and wins; all subsequent posts return false. Callers typically check the return value or use `TryReceive()` to get the winning value, ensuring only one initialization is used.

---

## Key Concepts Summary

1. **Single-Assignment**: First Post() wins, all others rejected
2. **No Cloning**: Always pass `null` for cloning function
3. **Graceful Rejection**: Post() returns false, doesn't throw
4. **Use Cases**: First-wins races, lazy init, signal broadcasting
5. **Not For Streaming**: Use BroadcastBlock for continuous values
6. **Completion**: Throws if no value posted before completion
7. **Immutable Broadcasting**: Same instance to all targets
8. **Thread-Safe**: Lock-free first-wins race resolution
