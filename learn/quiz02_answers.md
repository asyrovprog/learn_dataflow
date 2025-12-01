# BatchBlock & Grouping Data Quiz - Answers

**1. C) `int[]`**
   - **Reasoning:** `BatchBlock<T>` outputs arrays of type `T[]`. So `BatchBlock<int>` produces `int[]`, `BatchBlock<string>` produces `string[]`, etc.

**2. D) You must call `TriggerBatch()` to output the incomplete batch**
   - **Reasoning:** By default, `BatchBlock` only outputs when it reaches the configured batch size. If you have fewer items when calling `Complete()`, you must explicitly call `TriggerBatch()` first to flush the incomplete batch. Otherwise, those items remain buffered and never get output.

**3. B) Greedy**
   - **Reasoning:** Greedy is the default mode. In greedy mode, `BatchBlock` immediately accepts messages as they arrive and batches them. Non-greedy mode must be explicitly configured via `GroupingDataflowBlockOptions`.

**4. B) Immediately as they arrive**
   - **Reasoning:** In greedy mode, the block accepts all offered messages right away and buffers them until a full batch is ready. This is the most common and performant mode for simple batching scenarios.

**5. B) Reduced number of transactions/round-trips**
   - **Reasoning:** Instead of executing 100 separate INSERT statements (100 transactions, 100 network round-trips), you can batch them into a single bulk insert operation. This dramatically improves performance by reducing overhead.

**6. B) `Timer` to call `TriggerBatch()`**
   - **Reasoning:** The time-based batching pattern combines `BatchBlock` with a `Timer` that periodically calls `TriggerBatch()`. This ensures batches are processed even if the count threshold isn't reached, preventing items from sitting in the buffer indefinitely.

**7. B) When order of processing matters critically**
   - **Reasoning:** When batches are processed in parallel (`MaxDegreeOfParallelism > 1`), they can complete out of order. If you're processing bank transactions, state machine events, or any sequence-dependent operations, this can cause correctness issues. In such cases, sequential processing is required, which negates the performance benefits of batching.

**8. B) Coordination across multiple sources before accepting any messages**
   - **Reasoning:** Non-greedy mode (`Greedy = false`) is used when you need to coordinate multiple input sources. The block waits until it can collect exactly `batchSize` messages from all linked sources before accepting any of them. This is useful for scenarios like join operations where you need one message from each of multiple streams.
