# Quiz06 – Dynamic Routing & Predicate-Based Flow Control

Answer format:
Y/N questions: reply Y or N
Multiple choice (A–D): reply with single letter
Multi-select (M:): reply with concatenated letters (e.g. AC) – order doesn’t matter

1. Y/N: In an exclusive routing setup using a BufferBlock with multiple LinkTo predicates, the first predicate returning true claims the message and later predicates are not evaluated.
2. A–D: Which block is best for multi-cast selective distribution (one message may go to several targets)?
   A. BufferBlock<T>
   B. BroadcastBlock<T>
   C. TransformBlock<TIn,TOut>
   D. ActionBlock<T>
3. Y/N: A BroadcastBlock will deliver a message to exactly one linked target whose predicate matches.
4. A–D: The safest way to avoid unbounded growth when some messages match no classification predicates is:
   A. Increase bounded capacity
   B. Add parallelism to targets
   C. Add a final LinkTo to DataflowBlock.NullTarget with predicate always true
   D. Call Complete() earlier
5. M: Which of the following are good reasons to keep predicates pure and cheap?
   A. Prevent routing bottlenecks
   B. Maintain determinism & testability
   C. Enable higher MaxDegreeOfParallelism inside predicates
   D. Avoid blocking source delivery
6. A–D: You need to expand an input item into zero or more output items as part of routing (filter out some). The most direct block choice is:
   A. ActionBlock<T>
   B. TransformBlock<TIn,TOut>
   C. TransformManyBlock<TIn,TOut>
   D. BatchBlock<T>
7. Y/N: Completion propagation semantics change when predicates are used; branches with unmatched items will never complete unless manually completed.
8. M: When dynamically linking/unlinking targets at runtime, which practices help maintain pipeline stability?
   A. Store IDisposable returned by LinkTo for later unlink
   B. Mutate shared global state in predicates to reflect configuration
   C. Document invariants about routing order
   D. Provide a fallback NullTarget during reconfiguration

Provide your answers in order as a sequence (e.g. Y B N C ABD C N ACD). I will score immediately.
