# Custom Blocks & Encapsulation Quiz (Retry Variant)

Targeted follow-up focusing on missed concepts. Answer formats identical to original.

1. Which interfaces are exposed by an encapsulated block returned from `DataflowBlock.Encapsulate(entry, exit)`?
   A) ISourceBlock from entry, ITargetBlock from exit
   B) ITargetBlock from entry, ISourceBlock from exit
   C) IDataflowBlock only
   D) IPropagatorBlock from both

2. Multi-select: Valid reasons to encapsulate a mini-graph into a custom block:
   A) Repeated wiring pattern appears in several places
   B) You want to hide complexity behind a simpler abstraction
   C) You plan to frequently alter the public API shape each release
   D) Provide higher-level business semantics
   E) Improve unit test isolation on composite behavior
   F) Gain automatic time-travel debugging support

3. True/False: Encapsulation itself adds new ordering guarantees beyond those configured in internal blocks.
   Y) True
   N) False

4. Multi-select: Checklist items before returning a custom encapsulated block:
   A) PropagateCompletion on all internal links
   B) No internal block instance leaked externally
   C) Document thread-safety assumptions if non-trivial
   D) Provide configuration parameters for key behaviors
   E) Swallow all exceptions inside internal blocks
   F) Unit test completion and output

5. Why is including all valid checklist items important when publishing a reusable custom block?
   A) Ensures lifecycle correctness and consumer decoupling
   B) Improves GC compaction speed
   C) Guarantees ordering semantics regardless of internal settings
   D) Eliminates need for unit tests entirely

6. Which change fixes an incorrect multi-select answer where only A and B were chosen for reasons to encapsulate?
   A) Add E only
   B) Add D and E
   C) Remove B
   D) Add F

Provide answers: `1:B 2:A,B,...` etc.
