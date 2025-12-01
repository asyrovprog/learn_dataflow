# Lab 10 Reference Guide

## Hints

### TODO N1: Implement OfferMessage with Priority Storage

**Hint 1**: Validate `messageHeader.IsValid` first - throw `ArgumentException` if invalid.

**Hint 2**: Check `_decliningPermanently` flag under lock:
```csharp
lock (_lock)
{
    if (_decliningPermanently)
        return DataflowMessageStatus.DecliningPermanently;
}
```

**Hint 3**: Handle `consumeToAccept` protocol:
```csharp
if (consumeToAccept && source != null)
{
    messageValue = source.ConsumeMessage(messageHeader, this, out bool consumed);
    if (!consumed)
        return DataflowMessageStatus.NotAvailable;
}
```

**Hint 4**: Create `PriorityMessage<T>` with incrementing `_sequenceNumber` for FIFO:
```csharp
lock (_lock)
{
    var priorityMsg = new PriorityMessage<T>(
        messageValue.Priority,
        messageValue.Value,
        _sequenceNumber++);  // Increment for FIFO within same priority
    _priorityBuffer.Add(priorityMsg);
}
```

**Hint 5**: Return `DataflowMessageStatus.Accepted` at the end.

---

### TODO N2: Implement PropagateMessages

**Hint 1**: Use a while loop to process all messages in priority order.

**Hint 2**: Get the highest priority message (first in SortedSet):
```csharp
PriorityMessage<T>? messageToSend = null;
lock (_lock)
{
    if (_priorityBuffer.Count == 0) break;
    messageToSend = _priorityBuffer.First();  // Highest priority
}
```

**Hint 3**: Try offering to all targets (copy list first to avoid modification issues):
```csharp
ITargetBlock<T>[] targetsCopy;
lock (_lock)
{
    targetsCopy = _targets.ToArray();
}

foreach (var target in targetsCopy)
{
    var header = new DataflowMessageHeader(Interlocked.Increment(ref _nextMessageId));
    var status = target.OfferMessage(header, messageToSend.Value, this, consumeToAccept: false);
    
    if (status == DataflowMessageStatus.Accepted)
    {
        // Remove from buffer and break to next message
        lock (_lock)
        {
            _priorityBuffer.Remove(messageToSend);
        }
        offered = true;
        break;
    }
}
```

**Hint 4**: If no target accepts, break the loop (messages are stuck).

**Hint 5**: Call `CheckCompletion()` at the end.

---

### TODO N3: Implement Completion and Fault Propagation

**Hint 1**: `Complete()` method:
```csharp
public void Complete()
{
    lock (_lock)
    {
        _decliningPermanently = true;  // Stop accepting new messages
    }
    
    PropagateMessages();  // Drain buffer first
}
```

**Hint 2**: `Fault(Exception)` method:
```csharp
public void Fault(Exception exception)
{
    lock (_lock)
    {
        _decliningPermanently = true;
    }
    
    _completion.TrySetException(exception);
    
    // Propagate fault to all targets
    ITargetBlock<T>[] targetsCopy;
    lock (_lock)
    {
        targetsCopy = _targets.ToArray();
    }
    
    foreach (var target in targetsCopy)
    {
        target.Fault(exception);
    }
}
```

**Hint 3**: `CheckCompletion()` helper:
```csharp
private void CheckCompletion()
{
    bool shouldComplete;
    lock (_lock)
    {
        shouldComplete = _decliningPermanently && _priorityBuffer.Count == 0;
    }
    
    if (shouldComplete && !_completion.Task.IsCompleted)
    {
        _completion.TrySetResult();
        
        // Propagate completion to all targets
        ITargetBlock<T>[] targetsCopy;
        lock (_lock)
        {
            targetsCopy = _targets.ToArray();
        }
        
        foreach (var target in targetsCopy)
        {
            target.Complete();
        }
    }
}
```

---

## Common Mistakes

1. **Not incrementing `_sequenceNumber`** - Results in non-deterministic FIFO ordering within same priority
2. **Propagating immediately in `OfferMessage`** - Messages propagate before all are buffered; defeats priority sorting
3. **Not checking `_decliningPermanently` in `OfferMessage`** - Block accepts messages after Complete()
4. **Forgetting to call `PropagateMessages()` in `Complete()`** - Buffer doesn't drain before completion
5. **Not locking when accessing shared state** - Race conditions with `_priorityBuffer` and `_targets`

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter10;

// Helper class to store messages with priority and sequence
public class PriorityMessage<T> : IComparable<PriorityMessage<T>>
{
    public int Priority { get; }
    public T Value { get; }
    public long Sequence { get; }

    public PriorityMessage(int priority, T value, long sequence)
    {
        Priority = priority;
        Value = value;
        Sequence = sequence;
    }

    // Sort by priority descending, then by sequence ascending (FIFO within same priority)
    public int CompareTo(PriorityMessage<T>? other)
    {
        if (other == null) return 1;
        
        // Higher priority first (descending)
        int priorityCompare = other.Priority.CompareTo(Priority);
        if (priorityCompare != 0) return priorityCompare;
        
        // Within same priority, FIFO (ascending sequence)
        return Sequence.CompareTo(other.Sequence);
    }
}

public class PriorityBufferBlock<T> : IPropagatorBlock<(int Priority, T Value), T>
{
    private readonly SortedSet<PriorityMessage<T>> _priorityBuffer = new();
    private readonly List<ITargetBlock<T>> _targets = new();
    private readonly TaskCompletionSource _completion = new();
    private readonly object _lock = new();
    private bool _decliningPermanently = false;
    private long _nextMessageId = 0;
    private long _sequenceNumber = 0;

    // === ITargetBlock Implementation ===
    
    public DataflowMessageStatus OfferMessage(
        DataflowMessageHeader messageHeader,
        (int Priority, T Value) messageValue,
        ISourceBlock<(int Priority, T Value)>? source,
        bool consumeToAccept)
    {
        if (!messageHeader.IsValid)
            throw new ArgumentException("Invalid message header");

        lock (_lock)
        {
            if (_decliningPermanently)
                return DataflowMessageStatus.DecliningPermanently;
        }

        if (consumeToAccept && source != null)
        {
            messageValue = source.ConsumeMessage(messageHeader, this, out bool consumed);
            if (!consumed)
                return DataflowMessageStatus.NotAvailable;
        }

        lock (_lock)
        {
            var priorityMsg = new PriorityMessage<T>(
                messageValue.Priority,
                messageValue.Value,
                _sequenceNumber++);
            _priorityBuffer.Add(priorityMsg);
        }

        return DataflowMessageStatus.Accepted;
    }

    // === ISourceBlock Implementation ===
    
    public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        lock (_lock)
        {
            _targets.Add(target);
        }

        // Don't propagate immediately on link - wait for Complete()
        // This allows messages to buffer first

        return new Unlinker(() =>
        {
            lock (_lock)
            {
                _targets.Remove(target);
            }
        });
    }

    public T? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
    {
        messageConsumed = false;
        return default;
    }

    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target) => false;

    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target) { }

    // === IDataflowBlock Implementation ===
    
    public Task Completion => _completion.Task;

    public void Complete()
    {
        lock (_lock)
        {
            _decliningPermanently = true;
        }

        // Propagate all buffered messages before completing
        PropagateMessages();
    }

    public void Fault(Exception exception)
    {
        lock (_lock)
        {
            _decliningPermanently = true;
        }

        _completion.TrySetException(exception);

        ITargetBlock<T>[] targetsCopy;
        lock (_lock)
        {
            targetsCopy = _targets.ToArray();
        }

        foreach (var target in targetsCopy)
        {
            target.Fault(exception);
        }
    }

    // === Helper Methods ===
    
    private void PropagateMessages()
    {
        while (true)
        {
            PriorityMessage<T>? messageToSend = null;

            lock (_lock)
            {
                if (_priorityBuffer.Count == 0) break;
                messageToSend = _priorityBuffer.First();
            }

            bool offered = false;
            ITargetBlock<T>[] targetsCopy;

            lock (_lock)
            {
                targetsCopy = _targets.ToArray();
            }

            foreach (var target in targetsCopy)
            {
                var header = new DataflowMessageHeader(Interlocked.Increment(ref _nextMessageId));
                var status = target.OfferMessage(header, messageToSend.Value, this, consumeToAccept: false);

                if (status == DataflowMessageStatus.Accepted)
                {
                    offered = true;
                    lock (_lock)
                    {
                        _priorityBuffer.Remove(messageToSend);
                    }
                    break;
                }
            }

            if (!offered) break;
        }

        CheckCompletion();
    }

    private void CheckCompletion()
    {
        bool shouldComplete;
        lock (_lock)
        {
            shouldComplete = _decliningPermanently && _priorityBuffer.Count == 0;
        }

        if (shouldComplete && !_completion.Task.IsCompleted)
        {
            _completion.TrySetResult();

            ITargetBlock<T>[] targetsCopy;
            lock (_lock)
            {
                targetsCopy = _targets.ToArray();
            }

            foreach (var target in targetsCopy)
            {
                target.Complete();
            }
        }
    }

    private class Unlinker : IDisposable
    {
        private readonly Action _unlinkAction;
        public Unlinker(Action unlinkAction) => _unlinkAction = unlinkAction;
        public void Dispose() => _unlinkAction();
    }
}
```

</details>
