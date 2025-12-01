# Error Handling & Block Faults Quiz

**1. What are the three possible completion states for a dataflow block's `Completion` task?**
A) Success, Warning, Error
B) RanToCompletion, Faulted, Canceled
C) Complete, Failed, Aborted
D) Finished, Exception, Terminated

**2. When `PropagateCompletion = true` is set on a link, what else propagates besides completion?**
A) Only successful completion propagates
B) Configuration options propagate
C) Faults and cancellation also propagate
D) Nothing else propagates

**3. How are exceptions wrapped when thrown inside a dataflow block?**
A) They remain as the original exception type
B) Wrapped in `TaskCanceledException`
C) Wrapped in `AggregateException`
D) Wrapped in `DataflowException`

**4. Which error handling strategy allows the pipeline to continue processing even when individual items fail?**
A) Pipeline-level fault monitoring
B) Letting faults propagate with PropagateCompletion
C) Try-catch inside the block (item-level recovery)
D) Using CancellationToken

**5. What is the purpose of using link predicates for error handling?**
A) To improve performance
B) To route valid and invalid items to separate processing paths
C) To prevent exceptions from being thrown
D) To automatically retry failed operations

**6. If a block faults and `PropagateCompletion` is NOT set, what happens to downstream blocks?**
A) They also fault immediately
B) They continue running independently
C) They are canceled
D) They wait indefinitely

**7. What is the difference between a fault and a cancellation?**
A) Faults are intentional, cancellation is unexpected
B) Faults are unexpected errors, cancellation is intentional shutdown
C) They are the same thing
D) Faults only occur with network errors

**8. Which pattern is best for handling expected validation failures (like invalid user input)?**
A) Throw exceptions and catch at pipeline level
B) Let the block fault and propagate
C) Use Result types or predicates to route errors without throwing
D) Use CancellationToken to cancel invalid items
