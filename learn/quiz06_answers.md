# Quiz06 Answers & Explanations

1. Y – BufferBlock predicate routing is first-match wins; later predicates skipped to ensure exclusivity.
2. B – BroadcastBlock duplicates messages and applies each predicate independently enabling multi-cast selective distribution.
3. N – BroadcastBlock can send to multiple targets whose predicates match; not limited to one.
4. C – A NullTarget fallback consumes unmatched messages preventing accumulation; other options don’t address classification gaps correctly.
5. ABD – Cheap pure predicates avoid bottlenecks (A), keep logic deterministic & testable (B), and prevent source blocking (D). Parallelism inside predicate (C) isn’t an option—predicates are synchronous; heavy logic should be moved elsewhere.
6. C – TransformManyBlock returns 0..n outputs, naturally handling expansion and filtering (empty enumeration filters items).
7. N – Completion semantics remain; once source completes and messages drain, all linked targets complete (if PropagateCompletion used). Unmatched items are either processed or discarded via fallback so branches still complete.
8. ACD – Keep IDisposable handles (A), document invariants (C), and maintain fallback during transitions (D). Mutating shared global state inside predicates (B) risks race conditions & instability.

Scoring: 1 point per question; passing threshold 80% (>=6.4 ⇒ 7 correct). If below threshold, you can request explain for targeted recap.