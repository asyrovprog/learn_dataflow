# Dynamic Routing & Predicate-Based Flow Control in TPL Dataflow

## 1. Intuition
Dynamic routing lets each message take only the path it needs. Predicates attached to `LinkTo` decide where data travels, reducing wasted work.

## 2. Predicate Linking Basics
```csharp
source.LinkTo(targetA, m => m.Kind == Kind.High);
source.LinkTo(targetB, m => m.Kind == Kind.Normal);
source.LinkTo(DataflowBlock.NullTarget<Message>(), m => true); // fallback drop
```
First match wins for non-broadcast sources. Always include a fallback to avoid buildup.

## 3. Exclusive vs Multi-Cast
`BufferBlock` routes to ONE target (first predicate true). `BroadcastBlock` evaluates ALL predicates and sends copies to each that match (use for taps: metrics/audit).

## 4. Patterns
- Priority split: order predicates High → Invalid → Normal.
- Audit tap: BroadcastBlock in parallel, discard non-audit via NullTarget.
- Expansion: TransformManyBlock returns 0..n outputs (natural filter when returning empty).
- Dynamic subscription: keep `IDisposable` from `LinkTo` to unlink later.

## 5. Completion & Faults
Completion still flows downstream; routing does not change semantics. Faults only impact branches downstream of faulting block when `PropagateCompletion` used. Independent branches can continue.

## 6. Performance Notes
- Keep predicates pure & cheap (no I/O).
- Heavy classification? Precompute a Type field earlier.
- Discard early to control memory.
- Monitor per-route counts for skew.

## 7. Testing Strategy
Feed deterministic inputs, assert counts per lane, include overlap edge case, validate fallback catches unknown types.

## 8. Common Mistakes
| Mistake | Fix |
|---------|-----|
| Overlapping predicates | Reorder or make mutually exclusive |
| No fallback | Add NullTarget discard |
| Predicate side-effects | Remove; keep pure |
| Heavy logic inside predicate | Move to TransformBlock earlier |
| Forget completion wiring | Use `PropagateCompletion` or manually call Complete |

## 9. Exercise
Events `{ Severity, Category, Payload }`:
1. High severity → fast lane (parallelism 8) + also audit.
2. Category `Metrics` → TransformMany expands numeric samples → aggregate.
3. Invalid (missing payload) → count & drop early.
4. Others → normal lane.
Provide final counts per route.

## 10. Summary
Use ordered predicates for classification, BroadcastBlock for multi-cast, TransformMany for structural filtering/expansion, NullTarget for safe discard, keep predicates cheap, wire completion carefully.
