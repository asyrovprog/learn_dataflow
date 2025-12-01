# Quiz 08: DataflowBlock.Encapsulate & Advanced Composition

Answer the following questions to test your understanding of DataflowBlock.Encapsulate and advanced composition patterns.

**Format**:
- Y/N questions: Answer with Y or N
- Multiple choice (A-D): Answer with the letter
- Multi-select (M:): Answer with multiple letters like "AC" or "BD"

---

## Q1: Method Signature (A-D)
What does `DataflowBlock.Encapsulate<TInput, TOutput>()` return?

A) `ITargetBlock<TInput>`  
B) `ISourceBlock<TOutput>`  
C) `IPropagatorBlock<TInput, TOutput>`  
D) `Task<TOutput>`

**Answer**:

---

## Q2: Required Parameters (A-D)
Which two blocks must you provide to `DataflowBlock.Encapsulate()`?

A) A source block and a target block  
B) A target block (entry) and a source block (exit)  
C) Two propagator blocks  
D) An input buffer and an output buffer

**Answer**:

---

## Q3: Completion Propagation (Y/N)
Must all internal links in an encapsulated pipeline use `PropagateCompletion = true` for the encapsulated block to complete properly?

**Answer**:

---

## Q4: Exception Handling (A-D)
What happens if an internal TransformBlock throws an unhandled exception in an encapsulated pipeline?

A) Only that block faults; others continue processing  
B) The exception is caught and logged automatically  
C) The entire encapsulated pipeline faults and stops processing  
D) The exception is converted to a Result<T> type

**Answer**:

---

## Q5: Production Patterns (M:)
Which patterns are recommended for handling validation errors in production encapsulated pipelines? (Select all that apply)

A) Throw exceptions to fail fast  
B) Return Result<T> types with success/failure states  
C) Filter invalid items (return null) and use predicates  
D) Route invalid items to a separate handler/DLQ  
E) Let exceptions propagate to fault the pipeline

**Answer**:

---

## Q6: Interface Exposure (A-D)
What is the primary benefit of encapsulating a complex multi-stage pipeline?

A) Better performance through parallelization  
B) Hiding internal complexity and exposing only input/output interfaces  
C) Automatic error recovery  
D) Reduced memory consumption

**Answer**:

---

## Q7: Configuration Pattern (Y/N)
Is it a good practice to accept configuration options (like MaxDegreeOfParallelism, BoundedCapacity) as parameters when creating encapsulated blocks?

**Answer**:

---

## Q8: Internal Block Visibility (A-D)
After encapsulation, can consumers access the internal blocks (like intermediate TransformBlocks) of the encapsulated pipeline?

A) Yes, through reflection  
B) Yes, if you return them from the factory method  
C) No, they are completely hidden by the IPropagatorBlock interface  
D) Only if PropagateCompletion is false

**Answer**:

---

## Q9: Diagnostics Pattern (M:)
Which approaches can you use to expose diagnostics/metrics from an encapsulated block? (Select all that apply)

A) Pass a diagnostics object that blocks can update  
B) Return diagnostics as part of the output type  
C) Use a separate observable stream for metrics  
D) Store state in static variables  
E) Expose internal blocks to consumers

**Answer**:

---

## Q10: Retry Logic (A-D)
In a retry pattern within an encapsulated block, what's the best approach to handle items that fail all retry attempts?

A) Throw an exception to fault the pipeline  
B) Return a Result<T> with failure information  
C) Silently drop the item  
D) Block indefinitely until success

**Answer**:

---

## Scoring
- 10/10 correct: 100% ✅ Perfect!
- 9/10 correct: 90% ✅ Excellent
- 8/10 correct: 80% ✅ Pass
- <8 correct: <80% ⚠️ Review recommended
