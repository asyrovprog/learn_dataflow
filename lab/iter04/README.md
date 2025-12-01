# Lab 04: Dual Path Fork/Join Custom Block (Encapsulation)

## Overview
You will build a reusable custom dataflow block that internally fans out each input string to two parallel transformation paths (length scoring and vowel counting) and then joins results back into a composite output record.

The goal is to practice: 
1. Designing a small internal pipeline (fork -> transform -> join)
2. Encapsulating multiple blocks behind a single `IPropagatorBlock<string, ProcessedItem>` using `DataflowBlock.Encapsulate`
3. (Optional) Adding lightweight diagnostics/statistics for the composite block.

### Input / Output
Input: `string` (e.g. "Hello")
Output: `ProcessedItem { string Content, int LengthScore, int VowelCount }`

### Internal Flow
```
Buffer (input) -> Broadcast -> (Transform A: length) -> Join ┐
                                 (Transform B: vowels) -> Join ┘ -> Transform (assemble record)
```

---
## TODO N1 – Build Fork/Join Internal Pipeline
Implement `BuildInternalForkJoinPipeline()` to create and link the internal blocks:
- A `BufferBlock<string>` for input buffering.
- A `BroadcastBlock<string>` to duplicate messages.
- Two `TransformBlock<string,(string,int)>` blocks:
  - Length path: `(content, content.Length)`
  - Vowel path:  `(content, CountVowels(content))`
- A `JoinBlock<(string,int),(string,int)>` to pair results from both paths.
- A final `TransformBlock<Tuple<(string,int),(string,int)>, ProcessedItem>` assembling the `ProcessedItem`.

Requirements:
- Use `EnsureOrdered = true` to keep proper pairing.
- Propagate completion from input through each link.
- Return the input target and final output source blocks in a tuple.

---
## TODO N2 – Encapsulate Composite as Custom Block
Implement `CreateForkJoinBlock()` that calls `BuildInternalForkJoinPipeline()` and returns a single `IPropagatorBlock<string, ProcessedItem>` using `DataflowBlock.Encapsulate`.

Requirements:
- Ensure completion propagation (calling `.Complete()` on the returned block signals internal graph).
- Expose only one target and one source; hide internal implementation details.
- Provide graceful fault handling: if an internal transform throws, fault the encapsulated block.

---
## TODO N3 – Add Diagnostics (Optional)
Add simple statistics collection:
- Total messages processed
- Average length
- Average vowel count

Expose via `ForkJoinDiagnostics GetDiagnostics()` or a property.

---
## Hints
- `BroadcastBlock<T>` duplicates without consuming; safe to link to multiple targets.
- `JoinBlock<T1,T2>` pairs outputs sequentially; ordering matters.
- `DataflowBlock.Encapsulate(target, source)` builds a custom propagator.
- Use `LinkTo(..., new DataflowLinkOptions { PropagateCompletion = true })` everywhere.
- Keep transformations pure for determinism.

---
## Testing Expectations
The test harness will:
1. Validate internal pipeline produces expected metrics (TODO N1)
2. Validate encapsulated block processes multiple inputs correctly (TODO N2)
3. (If implemented) check diagnostics consistency (TODO N3)

Failures will mention: `TODO[N1] not satisfied – see README section 'TODO N1 – Build Fork/Join Internal Pipeline'` etc.

---
## Completion Checklist
Before marking lab complete:
- Both TODOs (N1,N2) implemented and tests PASS.
- Optional N3 implemented (bonus).
- No dangling `NotImplementedException`.
- Understand encapsulation trade-offs (reusability, abstraction boundary, fault isolation).
