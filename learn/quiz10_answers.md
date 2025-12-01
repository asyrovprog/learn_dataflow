# Quiz 10 Answer Key: Custom IPropagatorBlock Implementation

---

## Q1. What interfaces must a class implement to be a full IPropagatorBlock<TIn, TOut>? (Multi-select: M:)

**Correct Answer:** ABC

**Explanation:**
- A) ✅ ITargetBlock<TIn> - Required for input side (OfferMessage)
- B) ✅ ISourceBlock<TOut> - Required for output side (LinkTo, ConsumeMessage, etc.)
- C) ✅ IDataflowBlock - Base interface with Completion, Complete(), Fault()
- D) ❌ IPropagatorBlock inherits from A, B, C - you implement those three, not IPropagatorBlock directly
- E) ❌ Not related to TPL Dataflow

Note: IPropagatorBlock<TIn,TOut> itself is just: `interface IPropagatorBlock<TIn,TOut> : ITargetBlock<TIn>, ISourceBlock<TOut>`

---

## Q2. When should OfferMessage return DataflowMessageStatus.Declined? (Single: A-D)

**Correct Answer:** B

**Explanation:**
- A) ❌ This should return DecliningPermanently (not Declined)
- B) ✅ Declined means temporary rejection - buffer full but will drain, source should retry (push model)
- C) ❌ This should return Postponed (target will pull when ready)
- D) ❌ This should return NotAvailable

---

## Q3. What's the key difference between Declined and Postponed return values? (Single: A-D)

**Correct Answer:** B

**Explanation:**
- A) ❌ Both are temporary (DecliningPermanently is the permanent one)
- B) ✅ Declined = source retries pushing (backpressure); Postponed = target pulls later via reservation protocol
- C) ❌ Both can be used by greedy or non-greedy blocks depending on design
- D) ❌ Both require thread-safety; the difference is about control flow (push vs pull)

Key: Declined = "try again later", Postponed = "I'll call you when ready"

---

## Q4. Why does the reservation protocol require BOTH ReserveMessage AND ConsumeMessage? (Single: A-D)

**Correct Answer:** B

**Explanation:**
- A) ❌ Performance is not the primary reason
- B) ✅ Two-phase commit pattern: Reserve locks messages atomically, then Consume if got all needed, or Release if not. Prevents deadlock in multi-source coordination (e.g., JoinBlock needs messages from ALL inputs)
- C) ❌ Messages are not shared; each message goes to one target
- D) ❌ Locking is still needed; this is about coordination protocol

Real scenario: JoinBlock must get message from Source1 AND Source2. Reserve both first (locking them), then consume both atomically, or release both if can't get all.

---

## Q5. In OfferMessage, when consumeToAccept=true, what must the target do? (Single: A-D)

**Correct Answer:** B

**Explanation:**
- A) ❌ When consumeToAccept=true, messageValue parameter is NOT the final value
- B) ✅ Must call source.ConsumeMessage to complete two-phase protocol; this overwrites messageValue via 'out' parameter
- C) ❌ Postponed is a different scenario (non-greedy waiting)
- D) ❌ Null check is good practice but not the protocol requirement

consumeToAccept=true → linked block offering → must call ConsumeMessage  
consumeToAccept=false → direct Post() → use messageValue directly

---

## Q6. Is there a built-in timeout/expiration for reserved messages in TPL Dataflow? (Y/N)

**Correct Answer:** N

**Explanation:**
No expiration mechanism exists. Reserved messages stay locked until:
- ConsumeMessage() is called
- ReleaseReservation() is called
- Source block completes/faults

**Risk:** If target reserves but never consumes/releases → permanent deadlock. Must always use try-finally to ensure cleanup.

---

## Q7. Which statements about the simple PropagateMessages() implementation are TRUE? (Multi-select: M:)

**Correct Answer:** ABDE

**Explanation:**
- A) ✅ No retry loop - if all targets decline, messages stay stuck
- B) ✅ Propagation only triggered by: new message, new link, or Complete()
- C) ❌ NOT suitable for production - needs continuous retry mechanism
- D) ✅ Real blocks use Task schedulers, timers, or event-driven callbacks
- E) ✅ Good for learning the protocol, but missing production robustness

The simple implementation demonstrates the protocol but would fail in scenarios where targets are busy and need retry logic.

---

## Q8. What's the main risk of holding a lock during async operations in custom blocks? (Single: A-D)

**Correct Answer:** B

**Explanation:**
- A) ❌ While performance can degrade, deadlock is the main risk
- B) ✅ Deadlock risk - if async continuation tries to acquire same lock, or if you call another block's method that calls back into yours
- C) ❌ Locks don't cause memory leaks
- D) ❌ Race conditions are prevented by locks, not caused by them

**Pattern:**
```csharp
// BAD
lock(_lock) { await ProcessAsync(); }  // DEADLOCK RISK

// GOOD
T item;
lock(_lock) { item = _queue.Dequeue(); }
await ProcessAsync(item);  // Process outside lock
```

---

## Q9. Which scenarios require building custom IPropagatorBlock instead of using Encapsulate? (Multi-select: M:)

**Correct Answer:** BCE

**Explanation:**
- A) ❌ Combining existing blocks → Use Encapsulate (perfect use case)
- B) ✅ Exotic buffering (priority queue, LRU) → Need custom internal storage
- C) ✅ Custom reservation logic → Encapsulate doesn't expose reservation methods
- D) ❌ Quick prototyping → Use Encapsulate (faster)
- E) ✅ Legacy system integration → May need custom protocols not achievable with standard blocks

Rule: Start with Encapsulate. Build custom only when behavior can't be achieved by composing built-in blocks.

---

## Q10. When implementing Complete(), what must happen before transitioning Completion task to RanToCompletion? (Single: A-D)

**Correct Answer:** C

**Explanation:**
- A) ❌ Targets complete as result of propagation, not a prerequisite
- B) ❌ This happens immediately in Complete(), but completion waits for buffer drain
- C) ✅ Must drain output buffer - all pending messages must be propagated before completing
- D) ❌ Cancellation is separate from completion

**Pattern:**
```csharp
public void Complete()
{
    _decliningPermanently = true;  // Stop accepting
    CheckCompletion();  // Will complete only when buffer empty
}

void CheckCompletion()
{
    if (_decliningPermanently && _buffer.Count == 0)
        _completion.TrySetResult(default);  // NOW we can complete
}
```

---

## Scoring Guide

- **10/10 (100%)**: Expert level - ready for complex custom block implementation
- **8-9/10 (80-90%)**: Strong understanding - minor clarifications needed
- **7/10 (70%)**: Pass - review key concepts (Declined/Postponed, reservation protocol)
- **<7/10 (<70%)**: Needs review - revisit learning materials and retry

---

## Key Takeaways

1. **Interface hierarchy**: IPropagatorBlock = ITargetBlock + ISourceBlock + IDataflowBlock
2. **Declined vs Postponed**: Push retry vs Pull reservation
3. **Two-phase protocol**: Reserve → Consume enables atomic multi-source coordination
4. **consumeToAccept**: true = call ConsumeMessage; false = use parameter directly
5. **No reservation expiration**: Must always cleanup (try-finally pattern)
6. **PropagateMessages limitation**: Simple version lacks retry, needs enhancement for production
7. **Lock safety**: Never hold locks during async operations
8. **Custom vs Encapsulate**: Build custom only for non-standard behavior
9. **Completion**: Drain buffer before completing
10. **Thread safety**: Critical for OfferMessage, LinkTo, buffer access
