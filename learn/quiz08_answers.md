# Quiz 08 Answer Key: DataflowBlock.Encapsulate & Advanced Composition

---

## Q1: Method Signature
**Correct Answer**: C

**Reasoning**: `DataflowBlock.Encapsulate<TInput, TOutput>()` returns an `IPropagatorBlock<TInput, TOutput>`, which is a block that acts as both a target (accepts input) and a source (provides output). This interface hides the internal pipeline complexity behind a single, composable block.

---

## Q2: Required Parameters
**Correct Answer**: B

**Reasoning**: Encapsulate requires exactly two parameters: a **target block** (the entry point where data enters) and a **source block** (the exit point where data leaves). Everything between these two points is hidden from consumers. The order matters: target first (entry), then source (exit).

---

## Q3: Completion Propagation
**Correct Answer**: Y

**Reasoning**: Yes, absolutely. All internal links must use `PropagateCompletion = true`. Without this, calling `Complete()` on the encapsulated block won't properly propagate through the internal pipeline to the exit source, causing the completion task to never complete. This is the most common mistake when building encapsulated blocks.

---

## Q4: Exception Handling
**Correct Answer**: C

**Reasoning**: When an internal TransformBlock throws an unhandled exception, that block enters a Faulted state. If `PropagateCompletion = true` (which it must be), the fault propagates to all downstream blocks, causing the entire encapsulated pipeline to fault and stop processing. This is why production systems typically avoid throwing exceptions and use Result types or filtering instead.

---

## Q5: Production Patterns
**Correct Answer**: BCD

**Reasoning**:
- **B) ✓** Result<T> types allow tracking success/failure without faulting
- **C) ✓** Filtering (return null) with predicates prevents invalid items from continuing
- **D) ✓** Routing to handlers/DLQ ensures nothing is lost and failures are tracked
- **A) ✗** Throwing exceptions faults the entire pipeline (fail-fast, not resilient)
- **E) ✗** Same as A - causes pipeline to fault

Production systems need resilience, so they handle errors gracefully without stopping the pipeline.

---

## Q6: Interface Exposure
**Correct Answer**: B

**Reasoning**: The primary benefit is abstraction - hiding internal complexity and exposing only clean input/output interfaces. Consumers see a simple `IPropagatorBlock<TInput, TOutput>` without needing to know about the multiple stages, routing logic, or internal transformations. This enables reusability, maintainability, and easier testing.

---

## Q7: Configuration Pattern
**Correct Answer**: Y

**Reasoning**: Yes, accepting configuration parameters (MaxDegreeOfParallelism, BoundedCapacity, timeouts, logging flags, etc.) makes encapsulated blocks flexible and reusable across different scenarios. This is a best practice that allows tuning performance and behavior without changing the block implementation.

---

## Q8: Internal Block Visibility
**Correct Answer**: C

**Reasoning**: No, internal blocks are completely hidden by the `IPropagatorBlock<TInput, TOutput>` interface. Consumers only see the entry and exit points. This is the entire point of encapsulation - hide implementation details and expose minimal surface area. The only way to expose them would be to explicitly return them from your factory method (bad practice).

---

## Q9: Diagnostics Pattern
**Correct Answer**: ABC

**Reasoning**:
- **A) ✓** Pass diagnostics object that internal blocks update (most common pattern)
- **B) ✓** Include metrics in output type (e.g., Result<T> with metadata)
- **C) ✓** Use separate observable/metrics stream (advanced but valid)
- **D) ✗** Static variables are not thread-safe and break encapsulation
- **E) ✗** Exposing internal blocks defeats the purpose of encapsulation

The key is providing visibility into behavior without breaking abstraction.

---

## Q10: Retry Logic
**Correct Answer**: B

**Reasoning**: Returning a `Result<T>` with failure information is the best approach. This allows:
- Pipeline continues processing other items (resilient)
- Failures are tracked and can be logged/routed
- Consumers can decide how to handle failures
- No data loss

Option A (throw exception) faults the pipeline. Option C (drop silently) loses data without tracking. Option D (block indefinitely) causes deadlocks.

---

## Key Concepts Summary

1. **Encapsulate Pattern**: Target (entry) + Source (exit) → IPropagatorBlock
2. **Completion**: PropagateCompletion=true on ALL internal links (critical!)
3. **Exception Handling**: Unhandled exceptions fault entire pipeline
4. **Production Patterns**: Use Result<T>, filtering, or routing instead of throwing
5. **Abstraction**: Hide internal complexity, expose minimal interfaces
6. **Configuration**: Accept config parameters for flexibility
7. **Visibility**: Internal blocks are hidden; use diagnostics objects for metrics
8. **Resilience**: Handle failures gracefully without stopping pipeline
9. **Reusability**: Build once, use everywhere with clean APIs
10. **Testing**: Test encapsulated blocks as single units
