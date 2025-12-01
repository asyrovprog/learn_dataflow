# Custom Blocks & Encapsulation Quiz

Answer format:
- Single choice: A/B/C/D
- Yes/No: Y / N
- Multi-select: M: (list letters, e.g. `M: A,C,E`)

---
**1. What does `DataflowBlock.Encapsulate(target, source)` primarily provide?**
A) A performance optimization that merges delegates
B) A way to expose a composed internal graph as a single propagator block
C) Automatic retry and fault isolation logic
D) Deadlock detection between blocks

**2. Which two interfaces are effectively exposed after encapsulation?**
A) ISourceBlock (from target) and ITargetBlock (from source)
B) ITargetBlock (from target) and ISourceBlock (from source)
C) IDataflowBlock and IDisposable
D) IReceivableSourceBlock and IAsyncEnumerable

**3. After calling `Complete()` on an encapsulated block, what must be true for internal blocks to finish correctly?**
A) They all have MaxDegreeOfParallelism = 1
B) All links inside used `PropagateCompletion = true`
C) Internal blocks were created with `EnsureOrdered = true`
D) You called `Fault(Exception)` manually first

**4. Select all good reasons to encapsulate (multi-select).**
A) Repeated wiring pattern across codebase
B) Need to hide internal complexity
C) Want to change public API shape on every release
D) Provide higher-level business semantic
E) Isolate unit tests per composite behavior
F) Achieve time-travel debugging

**5. True or False: Encapsulation adds new ordering guarantees beyond individual blocks.**
Y) True
N) False

**6. Which scenario most strongly indicates you should implement a custom interface (not just encapsulate)?**
A) You need a priority-aware scheduling queue
B) You have two TransformBlocks already linked
C) You need to filter even numbers
D) You will batch items using BatchBlock

**7. In an encapsulated Validate → Transform → Log pattern, where should logging side-effects occur?**
A) Outside the encapsulated block by observing Completion
B) Inside a dedicated ActionBlock linked via predicate from validation
C) Inside the Transform delegate exclusively
D) Only in a continuation of the Completion Task

**8. What happens if an internal block faults inside the encapsulated graph?**
A) Fault is hidden; outer block always completes successfully
B) Outer block's `Completion` becomes Faulted
C) Downstream consumers silently stop receiving items without task fault
D) Outer block auto-retries three times

**9. Multi-select: Which items belong in a creation checklist before returning a custom encapsulated block?**
A) Verify PropagateCompletion on all internal links
B) Ensure no internal block references escape
C) Document thread-safety assumptions
D) Provide configuration parameters for key behaviors
E) Wrap every internal block in try/catch that swallows exceptions
F) Add a unit test for completion and output

**10. Advantage of returning the interface type (`IPropagatorBlock<TIn,TOut>`) instead of concrete types?**
A) Enables easier mocking & substitution
B) Improves raw throughput automatically
C) Avoids need for PropagateCompletion
D) Eliminates memory allocations

---
Provide your answers in a list format, e.g.: `1:B 2:B 3:B ...`.
