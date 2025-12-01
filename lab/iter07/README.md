# Lab 07 - Start Signal Coordinator

## Overview
Build a workflow coordinator that uses **WriteOnceBlock** to broadcast a start signal to multiple workers. Workers wait for the signal before beginning their tasks, demonstrating single-assignment semantics and immutable broadcasting patterns.

## Scenario
Your system has multiple worker tasks that need to start simultaneously after initialization completes. Requirements:
1. Workers must wait for a start signal before beginning work
2. The start signal is broadcast once to all workers
3. Track which workers have started and completed
4. Ensure proper completion propagation

## TODO N1 – Implement Start Signal Block

**Objective**: Create a WriteOnceBlock that accepts a single start signal and broadcasts it to all waiting workers.

**Requirements**:
- Use WriteOnceBlock<bool> to represent the start signal
- Configure with appropriate options (no cloning function needed)
- Provide method to broadcast the start signal
- Handle multiple broadcast attempts gracefully

**Key Concepts**:
- WriteOnceBlock construction with `null` cloning function
- Single-assignment semantics (first Post wins)
- Thread-safe signal broadcasting

## TODO N2 – Implement Worker Task Logic

**Objective**: Create worker tasks that wait for the start signal, perform work, and report completion.

**Requirements**:
- Each worker must call `ReceiveAsync()` on the start signal block
- Workers should log when they start and complete
- Simulate work with async delay
- Track started and completed worker counts
- Handle graceful cancellation

**Key Concepts**:
- `ReceiveAsync()` for waiting on WriteOnceBlock
- Async/await patterns
- Thread-safe counter increments
- Task coordination

## TODO N3 – Verify Coordination & Completion

**Objective**: Implement coordination logic that ensures all workers start after the signal and complete successfully.

**Requirements**:
- Wait for all worker tasks to complete
- Verify all workers received the signal
- Verify all workers completed their work
- Handle timeout scenarios (10 seconds max)
- Return true if all workers complete successfully

**Key Concepts**:
- Task.WhenAll for coordinating multiple tasks
- Task.WhenAny for timeout handling
- Completion verification
- Timeout patterns

## Expected Output
```
Coordinator: Creating 5 workers...
Worker 1: Waiting for start signal...
Worker 2: Waiting for start signal...
Worker 3: Waiting for start signal...
Worker 4: Waiting for start signal...
Worker 5: Waiting for start signal...

Coordinator: Broadcasting start signal...
Worker 1: Started! Performing work...
Worker 2: Started! Performing work...
Worker 3: Started! Performing work...
Worker 4: Started! Performing work...
Worker 5: Started! Performing work...

Worker 1: Completed!
Worker 2: Completed!
Worker 3: Completed!
Worker 4: Completed!
Worker 5: Completed!

✓ All workers started
✓ All workers completed
✓ Signal broadcast successful (returned true on first attempt)
✓ Second broadcast attempt rejected (returned false)
```

## Hints
- See `REF.md` for detailed guidance on WriteOnceBlock usage and worker coordination patterns
- Remember: WriteOnceBlock always uses `null` for cloning function
- Use `Post()` return value to verify signal was accepted
- Workers should use `ReceiveAsync()` to wait for the signal
