### C# TPL Dataflow Quiz - Answers

**1. C) TransformBlock<TInput, TOutput>**
   - **Reasoning:** TransformBlock is a propagator block that takes an input, transforms it, and offers it as an output to a linked block. ActionBlock is a target, BufferBlock is primarily a source/buffer, and BroadcastBlock copies data to multiple targets.

**2. B) LinkTo()**
   - **Reasoning:** The LinkTo() method is the standard way to form a connection between a source and a target block, creating the pipeline.

**3. B) To control how many items can be processed concurrently.**
   - **Reasoning:** MaxDegreeOfParallelism allows a block to process multiple items at the same time, which is key for improving throughput on multi-core systems.

**4. C) PropagateCompletion = true**
   - **Reasoning:** This option, set on a DataflowLinkOptions object, ensures that when a source block completes, it sends a completion signal down the pipeline to the linked target block.

**5. D) BufferBlock<T>**
   - **Reasoning:** While it can also be a target, BufferBlock is often used as a source because its primary function is to store a collection of messages that can be consumed by other blocks. The other options are either targets or propagators.

**6. B) Set MaxDegreeOfParallelism > 1 on the download block.**
   - **Reasoning:** Since downloading is an I/O-bound operation, allowing multiple downloads to happen concurrently (MaxDegreeOfParallelism > 1) is the most effective way to improve the performance of the overall pipeline.

**7. B) A mechanism where downstream blocks can slow down upstream blocks to prevent being overwhelmed.**
   - **Reasoning:** Backpressure is a core feature. If a downstream block (e.g., saving to a slow disk) cannot keep up, it will stop accepting new messages. This causes the upstream block's output buffer to fill, which in turn pauses the upstream block, preventing excessive memory usage.

**8. C) TransformManyBlock<TInput, TOutput>**
   - **Reasoning:** This block is specifically designed for 1-to-many transformations. It takes one input and can produce zero, one, or many output items from it.
