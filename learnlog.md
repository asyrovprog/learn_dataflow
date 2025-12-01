10/24/2025
- Step: learn
- Id: 01
- Status: completed
- Topic: C# DataFlow library (System.Threading.Tasks.Dataflow)
- Summary: Covered core concepts including dataflow blocks (Source, Target, Propagator), block linking, completion flow control, common block types (BufferBlock, ActionBlock, TransformBlock, etc.), configuration options, and practical examples of pipeline construction. Materials saved to learn/learn01.md.

10/24/2025
- Step: quiz
- Id: 01
- Status: completed
- Result: 100%
- Topic: C# DataFlow library (System.Threading.Tasks.Dataflow)
- Summary: Scored perfect on fundamentals quiz covering block types, linking, parallelism, completion propagation, backpressure, and use-case scenarios. Questions in learn/quiz01.md; answers in learn/quiz01_answers.md.

10/24/2025
- Step: lab
- Id: 01
- Status: completed
- Topic: C# DataFlow library (System.Threading.Tasks.Dataflow)
- Lab: lab/iter01
- Result: Tests passing (dotnet run)
- Summary: Completed text analytics pipeline using TransformBlock for metrics and ActionBlock for aggregation, demonstrating bounded capacity, controlled parallelism, and completion propagation.

10/24/2025
- Step: topic
- Id: 02
- Status: completed
- Topic: BatchBlock & Grouping Data in TPL Dataflow
- Summary: Selected foundational topic covering batching messages for efficient bulk processing, batch sizing strategies, and coordinating grouped data flows.

10/24/2025
- Step: learn
- Id: 02
- Status: completed
- Topic: BatchBlock & Grouping Data in TPL Dataflow
- Summary: Covered BatchBlock fundamentals including batch sizing, greedy vs non-greedy modes, TriggerBatch(), time-based batching patterns, bulk operations, coordination scenarios, and best practices. Materials saved to learn/learn02.md.

10/24/2025
- Step: quiz
- Id: 02
- Status: completed
- Result: 100%
- Topic: BatchBlock & Grouping Data in TPL Dataflow
- Summary: Perfect score on BatchBlock quiz covering output types, TriggerBatch(), greedy/non-greedy modes, time-based batching, bulk operations, ordering concerns, and coordination patterns. Questions in learn/quiz02.md; answers in learn/quiz02_answers.md.

10/24/2025
- Step: lab
- Id: 02
- Status: completed
- Topic: BatchBlock & Grouping Data in TPL Dataflow
- Lab: lab/iter02
- Result: Tests passing (dotnet run)
- Summary: Completed log aggregator pipeline using BatchBlock with time-based batching (50 entries OR 5 seconds), Timer integration, TriggerBatch() usage, and statistics tracking.

10/24/2025
- Step: topic
- Id: 03
- Status: completed
- Topic: Error Handling & Block Faults in TPL Dataflow
- Summary: Selected foundational topic covering exception handling in dataflow blocks, fault propagation mechanisms, recovery strategies, and graceful degradation patterns.

10/24/2025
- Step: learn
- Id: 03
- Status: completed
- Topic: Error Handling & Block Faults in TPL Dataflow
- Summary: Covered block fault states, fault propagation with PropagateCompletion, three error handling strategies (try-catch, predicate filtering, pipeline-level monitoring), AggregateException handling, cancellation vs faults, and best practices for resilient pipelines. Materials saved to learn/learn03.md.

10/24/2025
- Step: quiz
- Id: 03
- Status: completed
- Result: 100%
- Topic: Error Handling & Block Faults in TPL Dataflow
- Summary: Perfect score on error handling quiz covering block completion states, fault propagation, AggregateException, item-level vs pipeline-level error handling strategies, predicate routing, fault isolation, and cancellation vs faults. Questions in learn/quiz03.md; answers in learn/quiz03_answers.md.

10/24/2025
- Step: lab
- Id: 03
- Status: completed
- Topic: Error Handling & Block Faults in TPL Dataflow
- Lab: lab/iter03
- Result: Tests passing (dotnet run)
- Summary: Completed resilient data import pipeline processing CSV with validation, predicate-based routing for valid/invalid records, descriptive error messages, and statistics reporting. Successfully implemented item-level error recovery without pipeline faults.

10/26/2025
- Step: topic
- Id: 04
- Status: completed
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Summary: Selected foundational topic covering creation of custom dataflow blocks, encapsulating pipelines as reusable components, block composition patterns, and building higher-level abstractions.

10/26/2025
- Step: learn
- Id: 04
- Status: started
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Summary: Learning how to create reusable dataflow components by encapsulating pipelines, building custom blocks with Encapsulate method, and applying composition patterns for maintainable dataflow architectures.

10/26/2025
- Step: learn
- Id: 04
- Status: completed
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Summary: Added comprehensive materials with diagrams, heuristics, patterns (filter-transform, validate-route, throttle, fork-join), checklist, and exercise.

10/26/2025
- Step: quiz
- Id: 04
- Status: completed
- Result: 60%
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Summary: Quiz04 covering encapsulation purpose, exposed interfaces, completion prerequisites, use cases vs custom implementation, logging location, fault propagation, checklist items, and abstraction benefits. Questions in learn/quiz04.md; answers in learn/quiz04_answers.md.

10/26/2025
- Step: quiz-retry
- Id: 04
- Status: completed
- Result: 33%
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Summary: First retry attempt; incorrect on interface exposure, checklist completeness, checklist purpose rationale, and fix strategy question. Needs reinforcement on exposed interfaces (target/source pairing), reasons to encapsulate (adding D & E), full checklist (include configuration params), and lifecycle vs ordering guarantees.

10/27/2025
- Step: lab
- Id: 04
- Status: completed
- Topic: Custom Blocks & Encapsulation in TPL Dataflow
- Lab: lab/iter04
- Result: Tests passing (dotnet run)
- Summary: Completed dual-path fork/join custom block implementing internal pipeline assembly with TransformBlock for upper/lower case processing, Encapsulate method to expose unified IPropagatorBlock interface, and optional diagnostics tracking processed items.

10/27/2025
- Step: topic
- Id: 05
- Status: completed
- Topic: JoinBlock & Coordinated Data Flows
- Summary: Selected foundational topic covering synchronization of multiple data streams, greedy vs non-greedy joining modes, coordination of parallel data sources, and building complex multi-input workflows.

10/29/2025
- Step: learn
- Id: 05
- Status: completed
- Topic: JoinBlock & Coordinated Data Flows
- Summary: Completed comprehensive learning materials covering JoinBlock fundamentals including multi-input target interfaces, greedy vs non-greedy modes, ordering guarantees, JoinBlock variants (2-7 inputs), common patterns (ID-entity lookup, multi-source aggregation, synchronized fan-in), configuration options, completion and fault propagation, practical examples with Mermaid diagrams, best practices, common pitfalls, and exercise challenge. Materials available in learn/learn05.md.

10/29/2025
- Step: quiz
- Id: 05
- Status: completed
- Result: 87.5% (7/8)
- Topic: JoinBlock & Coordinated Data Flows
- Summary: Passed quiz with 87.5% covering JoinBlock synchronization behavior, greedy vs non-greedy modes, ordering guarantees, input limits, common patterns, completion conditions, and configuration for memory management. Missed Q7 on completion requirements (answered first input vs all inputs). Quiz questions in learn/quiz05.md; answer key in learn/quiz05_answers.md.

10/31/2025
- Step: lab
- Id: 05
- Status: completed
- Topic: JoinBlock & Coordinated Data Flows
- Lab: lab/iter05
- Result: Tests passing (dotnet run)
- Summary: Completed Multi-Source Weather Dashboard using JoinBlock<Weather, AirQuality, Traffic> to synchronize three independent data streams. Implemented async fetch methods with simulated delays (100ms, 150ms, 80ms), built pipeline with BufferBlock→BroadcastBlock→three TransformBlocks→JoinBlock→ActionBlock, used greedy mode for synchronization, and proper completion propagation. Successfully coordinated parallel data sources with FIFO ordering guarantees.

11/01/2025
- Step: topic
- Id: 06
- Status: completed
- Topic: Dynamic Routing & Predicate-Based Flow Control
- Summary: Selected topic focusing on conditional links, branching pipelines, selective propagation via predicates and LinkOptions, and completion propagation nuances for flexible dataflow architectures.

11/01/2025
- Step: learn
- Id: 06
- Status: started
- Topic: Dynamic Routing & Predicate-Based Flow Control
- Summary: Introduced predicate-based routing (exclusive vs multi-cast), key patterns (priority split, audit tap, expansion, dynamic subscription), completion/fault nuances, performance guidelines, testing, and common mistakes.

11/02/2025
- Step: learn
- Id: 06
- Status: completed
- Topic: Dynamic Routing & Predicate-Based Flow Control
- Summary: Finalized materials covering predicate semantics, exclusive vs multi-cast routing, TransformMany expansion, audit taps, completion/fault nuances, performance and testing patterns, and common pitfalls.

11/02/2025
- Step: quiz
- Id: 06
- Status: completed
- Result: 75%
- Topic: Dynamic Routing & Predicate-Based Flow Control
- Summary: Missed predicate motivation for avoiding source blocking (Q5 option D) and dynamic reconfiguration stability invariant documentation (Q8 option C). Needs reinforcement on reasons for cheap predicates (performance + blocking avoidance) and runtime link management best practices.

11/03/2025
- Step: lab
- Id: 06
- Status: completed
- Topic: Dynamic Routing & Predicate-Based Flow Control
- Lab: lab/iter06
- Result: Tests passing (dotnet run)
- Summary: Completed Smart Event Router implementing predicate-based routing with BroadcastBlock for multi-cast support. Successfully routed events to specialized handlers (Security, Performance, General) based on category and severity predicates, implemented audit tap for all Critical events, and verified proper completion propagation across all handlers. Demonstrated exclusive routing with multi-cast audit pattern.

11/03/2025
- Step: topic
- Id: 07
- Status: completed
- Topic: WriteOnceBlock & Immutable Broadcasting
- Summary: Selected foundational topic covering single-value blocks, race conditions in concurrent writes, immutable message broadcasting patterns, and single-assignment semantics as natural extension from BroadcastBlock.

11/03/2025
- Step: learn
- Id: 07
- Status: completed
- Topic: WriteOnceBlock & Immutable Broadcasting
- Summary: Covered WriteOnceBlock fundamentals including single-assignment semantics, first-wins race resolution, immutable broadcasting (no cloning), comparison with BroadcastBlock, common patterns (first-wins competition, lazy initialization, signal broadcasting), completion behavior, practical examples, and best practices. Materials saved to learn/learn07.md.

11/08/2025
- Step: quiz
- Id: 07
- Status: completed
- Result: 100%
- Topic: WriteOnceBlock & Immutable Broadcasting
- Summary: Perfect score on WriteOnceBlock quiz covering single-assignment semantics, cloning behavior (always null), Post() return values, comparison with BroadcastBlock, appropriate use cases (first-wins races, lazy init, signal broadcasting), completion behavior, instance sharing, and race condition patterns. Questions in learn/quiz07.md; answers in learn/quiz07_answers.md.

11/09/2025
- Step: lab
- Id: 07
- Status: completed
- Topic: WriteOnceBlock & Immutable Broadcasting
- Lab: lab/iter07
- Result: Tests passing (dotnet run)
- Summary: Completed Start Signal Coordinator using WriteOnceBlock to broadcast start signal to multiple workers. Implemented single-assignment signal block with null cloning function, worker tasks using ReceiveAsync() to wait for signal, thread-safe counter increments with Interlocked, and coordination logic with Task.WhenAll/WhenAny for timeout handling. Successfully demonstrated WriteOnceBlock single-assignment semantics (first Post wins, subsequent rejected) and immutable broadcasting to multiple concurrent consumers.

11/09/2025
- Step: topic
- Id: 08
- Status: completed
- Topic: DataflowBlock.Encapsulate & Advanced Composition
- Summary: Selected advanced topic covering sophisticated encapsulation patterns, building reusable dataflow libraries, multi-stage internal pipelines, exposing minimal interfaces, and advanced composition techniques building upon previous custom blocks knowledge.

11/09/2025
- Step: learn
- Id: 08
- Status: completed
- Topic: DataflowBlock.Encapsulate & Advanced Composition
- Summary: Covered DataflowBlock.Encapsulate fundamentals including method signature and core concept (wrapping target/source into IPropagatorBlock), advanced patterns (filter-transform-route, configurable blocks, fork-join with diagnostics, retry logic), completion propagation rules (PropagateCompletion=true requirement), best practices, multi-stage pipeline examples, testing strategies, and performance considerations. Discussed exception handling implications - throwing exceptions in transforms faults entire pipeline, production patterns use Result types, routing, or filtering instead. Materials saved to learn/learn08.md.

11/10/2025
- Step: quiz
- Id: 08
- Status: completed
- Result: 90% (9/10)
- Topic: DataflowBlock.Encapsulate & Advanced Composition
- Summary: Scored 90% on advanced composition quiz covering Encapsulate return type (IPropagatorBlock), required parameters (target entry + source exit), completion propagation (PropagateCompletion=true mandatory), exception handling (faults entire pipeline), production error patterns, interface abstraction benefits, configuration patterns, internal block visibility, diagnostics approaches, and retry logic. Missed Q5 on production patterns - selected BCDE instead of BCD (letting exceptions propagate faults pipeline, not a production pattern). Questions in learn/quiz08.md; answers in learn/quiz08_answers.md.

11/12/2025
- Step: lab
- Id: 08
- Status: completed
- Topic: DataflowBlock.Encapsulate & Advanced Composition
- Lab: lab/iter08
- Result: Tests passing (dotnet run)
- Summary: Completed Resilient Data Processor using Result<T> pattern for production-ready error handling. Implemented Result<T> record type for success/failure representation, ValidateOrder method with validation logic (amount > 0, email contains "@"), encapsulated 4-stage pipeline (validator → pricer → router → output) using DataflowBlock.Encapsulate, and thread-safe ProcessorStats class with Interlocked operations. Successfully demonstrated PropagateCompletion on all internal links, price discounting for orders > $1000, express/standard routing based on IsExpress flag, and resilient processing without pipeline faults (8 orders: 6 success, 2 validation failures).

11/12/2025
- Step: topic
- Id: 09
- Status: started
- Topic: CancellationToken Integration & Graceful Shutdown
- Summary: Selected topic covering cancellation propagation through dataflow pipelines, coordinated cancellation across multiple blocks, timeout patterns, partial completion handling, and resource cleanup strategies for production-ready dataflow applications.

11/14/2025
- Step: learn
- Id: 09
- Status: completed
- Topic: CancellationToken Integration & Graceful Shutdown
- Summary: Studied cooperative cancellation patterns (block-level vs pipeline-wide tokens), terminal states vs faults, graceful two-phase shutdown, timeout/linking strategies, OperationCanceledException handling, and cleanup/diagnostics patterns. Materials in learn/learn09.md.

11/14/2025
- Step: quiz
- Id: 09
- Status: completed
- Result: 50% (4/8)
- Topic: CancellationToken Integration & Graceful Shutdown
- Summary: Failed initial quiz attempt. Missed linked token child pipeline pattern (Q5), IDisposable wrapper cleanup pattern (Q6), and partial progress reporting requirements (Q8). Questions in learn/quiz09.md; answers in learn/quiz09_answers.md.

11/14/2025
- Step: quiz-retry
- Id: 09
- Status: completed
- Result: 87.5% (7/8)
- Topic: CancellationToken Integration & Graceful Shutdown
- Summary: Passed retry quiz covering token passing to delegates and options, queued item processing after cancellation, linked token child pipeline patterns, graceful shutdown sequence, OCE handling in delegates, CTS disposal timing, partial result tracking patterns, and Post() behavior after cancellation. Missed Q7 - selected BD instead of ABCD for partial progress patterns (missed Interlocked counters and token checking). Questions in learn/quiz09.md; answers in learn/quiz09_answers.md.

11/25/2025
- Step: lab
- Id: 09
- Status: completed
- Topic: CancellationToken Integration & Graceful Shutdown
- Lab: lab/iter09
- Result: Tests passing (dotnet run)
- Summary: Completed Cancellable File Processor implementing thread-safe progress tracking with Interlocked operations (_processedCount, _skippedCount, _failedCount), graceful two-phase shutdown with 5-second timeout (Complete() → WaitAsync → Cancel on timeout), CancellationToken propagation to both block options and async operations (File.ReadAllTextAsync, Task.Delay), early cancellation detection with IsCancellationRequested, and proper OperationCanceledException handling. Successfully demonstrated 4/4 tests: graceful shutdown, immediate cancellation, partial processing with accurate counts, and thread-safe parallel counter updates.

11/25/2025
- Step: topic
- Id: 10
- Status: completed
- Topic: Custom IPropagatorBlock Implementation
- Summary: Selected advanced topic covering building dataflow blocks from scratch without Encapsulate, implementing ISourceBlock and ITargetBlock interfaces directly, understanding message reservation and consumption protocol, custom block lifecycle management, and knowing when to build custom implementations versus using DataflowBlock.Encapsulate.

11/25/2025
- Step: learn
- Id: 10
- Status: completed
- Topic: Custom IPropagatorBlock Implementation
- Summary: Created comprehensive materials covering interface hierarchy (IPropagatorBlock combining ITargetBlock and ISourceBlock), OfferMessage implementation with DataflowMessageStatus return values (Declined vs Postponed patterns), message reservation protocol for non-greedy scenarios with two-phase commit pattern (Reserve/Consume/Release), LinkTo mechanics, thread-safety patterns, completion propagation, minimal pass-through block example, rate-limiting block implementation, testing strategies, common mistakes (holding locks during async, missing header validation, premature completion), PropagateMessages retry limitation, reservation expiration concerns, and when to build custom vs use Encapsulate. Materials saved to learn/learn10.md.

11/25/2025
- Step: quiz
- Id: 10
- Status: completed
- Result: 70% (7/10)
- Topic: Custom IPropagatorBlock Implementation
- Summary: Passed quiz with 70% covering interface implementation (ABC correct), DataflowMessageStatus semantics (Declined vs Postponed), reservation protocol for atomic coordination, consumeToAccept handling, no reservation expiration, PropagateMessages retry limitations. Missed Q8 (lock deadlock risk vs performance - selected A instead of B), Q9 (custom scenarios - missed reservation protocol option C), Q10 (completion requires buffer drain - selected B immediate flag instead of C drain buffer). Questions in learn/quiz10.md; answers in learn/quiz10_answers.md.

11/26/2025
- Step: lab
- Id: 10
- Status: completed
- Topic: Custom IPropagatorBlock Implementation
- Lab: lab/iter10
- Result: Tests passing (4/4 - dotnet run)
- Summary: Completed PriorityBufferBlock<T> implementing ISourceBlock and ITargetBlock interfaces. Successfully implemented OfferMessage with priority-based SortedSet storage using custom PriorityMessage comparer (priority descending, FIFO within priority), PropagateMessages draining buffer highest-priority-first with ConsumeMessage protocol, and Complete/Fault/CheckCompletion lifecycle with _decliningPermanently flag and TrySetResult/TrySetException for completion task. All tests passed: priority ordering (5→3→1), FIFO within same priority, completion after buffer drain, and declining new messages after Complete().

11/26/2025
- Step: lab
- Id: 11
- Status: completed
- Topic: Producer-Consumer Patterns Beyond Dataflow
- Lab: lab/iter11
- Result: Tests passing (4/4)
- Summary: Completed rate-limited API request queue using System.Threading.Channels. Successfully implemented bounded channel with backpressure (FullMode.Wait), TryWrite/WriteAsync enqueue patterns with throttled counter tracking, SemaphoreSlim rate limiting (10 req/sec with fire-and-forget Release after 1s delay), consumer loop with await foreach over ReadAllAsync, thread-safe Interlocked statistics (_successCount, _failedCount, _throttledCount), graceful cancellation handling, and pending task completion tracking. Demonstrated producer-consumer patterns with resource constraints and proper async coordination.

11/26/2025
- Step: topic
- Id: 11
- Status: completed
- Topic: Producer-Consumer Patterns Beyond Dataflow
- Summary: Selected topic covering System.Threading.Channels deep dive, comparing Channels vs Dataflow (when to use each), channel-based async streaming patterns, and backpressure strategies across different concurrency primitives.

11/26/2025
- Step: learn
- Id: 11
- Status: completed
- Topic: Producer-Consumer Patterns Beyond Dataflow
- Summary: Completed learning System.Threading.Channels fundamentals including bounded vs unbounded channels, reading/writing patterns (await foreach with ReadAllAsync, WaitToReadAsync, ReadAsync), completion signaling, Channels vs Dataflow comparison (storage vs storage+processing), performance characteristics (10x faster for simple producer-consumer), advanced patterns (multi-producer/consumer, fan-out broadcasting, async streaming with IAsyncEnumerable), backpressure strategies (bounded blocking, drop policies, TryWrite fallback, adaptive throttling), real-world log aggregator example with batch flushing, common pitfalls (forgetting Complete(), unbounded with slow consumer, not handling completion exceptions), and best practices (SingleReader/SingleWriter optimization, ValueTask usage, multi-producer completion coordination). Materials saved to learn/learn11.md.

11/26/2025
- Step: quiz
- Id: 11
- Status: completed
- Result: 70% (7/10)
- Topic: Producer-Consumer Patterns Beyond Dataflow
- Summary: Passed quiz covering Channel architecture (shared queue between Writer/Reader), DropOldest ring buffer behavior (removes from front, adds to back), Channels vs Dataflow primary difference (storage vs storage+processing), ReadAllAsync best practice, completion signaling consequences, TryWrite vs WriteAsync usage, performance benefits (10x faster for simple scenarios), SingleReader optimization (lock-free algorithms, not enforcement), multi-producer completion coordination (Interlocked.Decrement pattern), and bounded vs unbounded tradeoffs. Missed Q5 (forgot unbounded memory leak with slow consumer - answered AC instead of ACD), Q8 (confused SingleReader as enforcement vs optimization hint - answered A instead of B), Q9 (thought each producer calls Complete() - correct answer uses Interlocked counter). Questions in learn/quiz11.md; answers in learn/quiz11_answers.md.

11/26/2025
- Step: topic
- Id: 12
- Status: completed
- Topic: Reactive Extensions (Rx.NET)
- Summary: Selected topic covering Observable sequences and operators, hot vs cold observables, backpressure with Reactive, and combining Rx with async/await as next progression from dataflow and channel-based patterns.

11/29/2025
- Step: learn
- Id: 12
- Status: completed
- Topic: Reactive Extensions (Rx.NET)
- Summary: Completed learning materials covering Observable sequences, hot vs cold observables, core operators (Create, Range, Interval, Timer, Return, Empty, Never, Throw, Select, Where, SelectMany, Take, Skip, Throttle, Debounce, Buffer, Window, Merge, Concat, Zip, CombineLatest, Amb), subscription management, schedulers, backpressure strategies, error handling (Catch, Retry, OnErrorResumeNext), combining with async/await, common patterns (retry with exponential backoff, polling with cancellation, debounce user input), and when to use Rx vs Dataflow vs Channels. Materials in learn/learn12.md.
- Summary: Learning Rx.NET fundamentals including IObservable<T>/IObserver<T> interfaces (push-based dual of IEnumerable), creating observables (Return, Range, FromAsync, FromEventPattern, Create), subscribing with OnNext/OnError/OnCompleted, hot vs cold observables (unicast with new producer per subscriber vs multicast with shared producer), converting cold→hot with Publish()/Connect()/RefCount(), common operators (Where, Select, SelectMany, Scan, Throttle, Sample, Buffer, Merge, CombineLatest, Zip), backpressure strategies (throttle/sample drop, buffer batch, window nested), practical search autocomplete example, Rx vs async/await comparison, retry with exponential backoff, polling patterns, best practices (dispose subscriptions, use operators, Publish().RefCount() for sharing), and common pitfalls (multiple cold subscriptions, memory leaks, blocking in OnNext). Materials in learn/learn12.md.


11/29/2025
- Step: quiz
- Id: 12
- Status: completed
- Topic: Reactive Extensions (Rx.NET)

11/29/2025
- Step: lab
- Id: 12
- Status: started
- Topic: Reactive Extensions (Rx.NET)
- Lab: lab/iter12
- TODOs: 3 (N1: Create debounced search observable with Throttle/Where, N2: Transform to API calls with SelectMany/FromAsync, N3: Subscribe and handle results with proper disposal)
