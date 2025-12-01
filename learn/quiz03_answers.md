# Error Handling & Block Faults Quiz - Answers

**1. B) RanToCompletion, Faulted, Canceled**
   - **Reasoning:** These are the three possible states of a Task in .NET, which the `Completion` property returns. `RanToCompletion` means success, `Faulted` means an exception occurred, and `Canceled` means a cancellation was requested.

**2. C) Faults and cancellation also propagate**
   - **Reasoning:** When you set `PropagateCompletion = true`, not only does successful completion propagate downstream, but faults and cancellation signals also propagate. This means if an upstream block fails or is canceled, downstream blocks will also enter those states.

**3. C) Wrapped in `AggregateException`**
   - **Reasoning:** Dataflow blocks may process multiple items in parallel, so exceptions from multiple operations could occur. The TPL wraps these in `AggregateException` to hold one or more inner exceptions. You need to iterate through `InnerExceptions` to see the actual errors.

**4. C) Try-catch inside the block (item-level recovery)**
   - **Reasoning:** By handling exceptions inside the block's processing delegate with try-catch, you can catch errors for individual items, log them, return default values, and allow the pipeline to continue processing other items. The block itself doesn't fault, so the pipeline keeps running.

**5. B) To route valid and invalid items to separate processing paths**
   - **Reasoning:** Link predicates allow you to conditionally route messages based on criteria. For error handling, you can check if an item is valid and route it to a success path, or check if it's invalid (e.g., null, error result) and route it to an error-handling path. This avoids throwing exceptions entirely.

**6. B) They continue running independently**
   - **Reasoning:** Without `PropagateCompletion`, blocks are independent. If an upstream block faults, downstream blocks don't automatically know about it and will continue processing any buffered items or waiting for new items. You must manually coordinate completion/faults if needed.

**7. B) Faults are unexpected errors, cancellation is intentional shutdown**
   - **Reasoning:** A fault occurs when an exception is thrown—something went wrong unexpectedly. Cancellation is a deliberate, graceful shutdown request using `CancellationToken`. They're handled differently: faults may require error recovery, while cancellation is a normal operational state.

**8. C) Use Result types or predicates to route errors without throwing**
   - **Reasoning:** For expected validation failures (bad user input, invalid data format), throwing exceptions is expensive and semantically wrong—it's not an exceptional condition. Instead, return a Result type or use nullable/optional patterns, then route invalid items to an error-handling path using predicates. This keeps the pipeline running smoothly.
