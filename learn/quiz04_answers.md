# Custom Blocks & Encapsulation Quiz - Answers & Explanations

1. B) Encapsulation exposes a composed internal graph (entry target + exit source) as a single propagator.
2. B) The outer block exposes the target interface of the first (entry) and the source interface of the last (exit).
3. B) Without PropagateCompletion, calling `Complete()` won't finish downstream internal blocks, leaving them hanging.
4. A, B, D, E. (C is bad; frequent public API change harms stability. F irrelevant.)
5. N) False. Ordering is governed by internal blocks' options, encapsulation adds no new guarantees.
6. A) Priority scheduling requires custom implementation beyond composition.
7. B) Side-effects (logging of failures) belong in a separate ActionBlock routed by a predicate for clean separation.
8. B) Any internal fault propagates and outer `Completion` faults.
9. A, B, C, D, F. (E is harmful—never silently swallow all exceptions.)
10. A) Returning interface keeps consumers decoupled and supports mocking; none of the other options are benefits.

Score guidance: Each correct = 10%. >=80% proceed; <80% review explanations then retry variant.
