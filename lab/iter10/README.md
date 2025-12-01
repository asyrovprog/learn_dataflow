# Lab 10: Priority Buffer Block

Build a custom IPropagatorBlock that buffers messages with priority levels and propagates highest-priority items first.

---

## Scenario

You're building a task scheduling system where tasks have different priority levels. Higher priority tasks should be processed before lower priority ones, even if lower priority tasks arrived first.

**Requirements:**
- Messages have an integer priority (higher number = higher priority)
- Buffer stores messages sorted by priority
- Propagate highest-priority messages first
- Proper completion handling (drain buffer before completing)
- Thread-safe operations

---

## TODO N1 – Implement OfferMessage with Priority Storage

**Objective**: Accept messages with priority and store them in priority order.

**Requirements**:
1. Validate `messageHeader.IsValid` (throw ArgumentException if invalid)
2. Check `_decliningPermanently` flag → return `DecliningPermanently` if true
3. Handle `consumeToAccept` protocol:
   - If `consumeToAccept && source != null`: call `source.ConsumeMessage(messageHeader, this, out messageValue)`
   - If consume fails, return `NotAvailable`
4. Store message in `_priorityBuffer` (SortedSet<PriorityMessage<T>>)
5. Call `PropagateMessages()` to try sending to targets
6. Return `Accepted`

**Data Structure**: Use `SortedSet<PriorityMessage<T>>` where `PriorityMessage<T>` implements `IComparable` to sort by priority (descending) then by sequence number (ascending for FIFO within same priority).

---

## TODO N2 – Implement PropagateMessages to Send Highest Priority First

**Objective**: Offer buffered messages to linked targets, starting with highest priority.

**Requirements**:
1. Lock `_priorityBuffer` to get snapshot of messages
2. For each message (highest priority first):
   - Try offering to each target in `_targets` list
   - Create unique `DataflowMessageHeader` with `Interlocked.Increment(ref _nextMessageId)`
   - Call `target.OfferMessage(header, message.Value, this, consumeToAccept: false)`
   - If `Accepted`: remove from `_priorityBuffer` and break to next message
   - If `Declined`/`Postponed`: try next target
3. If no target accepts a message, stop propagating (buffer has messages that can't be sent yet)
4. After propagation loop, call `CheckCompletion()`

**Thread Safety**: Copy `_targets` list before iterating to avoid modification issues.

---

## TODO N3 – Implement Completion and Fault Propagation

**Objective**: Properly handle block lifecycle (Complete, Fault, Completion task).

**Requirements**:

### Complete() method:
1. Set `_decliningPermanently = true` (future OfferMessage calls return DecliningPermanently)
2. Call `CheckCompletion()` to see if block can complete now

### Fault(Exception) method:
1. Set `_decliningPermanently = true`
2. Transition `_completion` task to faulted state: `_completion.TrySetException(exception)`
3. Propagate fault to all linked targets: call `target.Fault(exception)` for each

### CheckCompletion() helper:
1. Check if `_decliningPermanently && _priorityBuffer.Count == 0`
2. If yes:
   - Transition `_completion` task: `_completion.TrySetResult(default)`
   - Propagate completion to all targets: call `target.Complete()` for each

**Thread Safety**: Use locks when checking/modifying `_priorityBuffer` and `_targets`.

---

## Expected Behavior

**Test 1**: Priority ordering - messages [P:1, P:5, P:3] → output [5, 3, 1]  
**Test 2**: FIFO within same priority - [P:2, P:2, P:2] → output in insertion order  
**Test 3**: Completion - block completes after draining all messages  
**Test 4**: Declining permanently - after Complete(), Post() returns false  

---

## Files

- `Task.cs` - Your implementation (fill TODOs)
- `Program.cs` - Test harness
- `README.md` - This file
- `REF.md` - Hints and reference solution
