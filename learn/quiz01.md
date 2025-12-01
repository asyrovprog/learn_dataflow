# C# TPL Dataflow Quiz

**1. Which block type is designed to both receive and send data, acting as a transformation step in a pipeline?**
A) `ActionBlock<T>`
B) `BufferBlock<T>`
C) `TransformBlock<TInput, TOutput>`
D) `BroadcastBlock<T>`

**2. To connect two dataflow blocks, which method should you use?**
A) `ConnectTo()`
B) `LinkTo()`
C) `SendTo()`
D) `ChainTo()`

**3. What is the primary purpose of the `MaxDegreeOfParallelism` option?**
A) To set the maximum number of items a block can buffer.
B) To control how many items can be processed concurrently.
C) To define the number of downstream blocks to link to.
D) To limit the total number of blocks in a pipeline.

**4. If you want a block to automatically signal completion to its linked targets when it is done, which option must be set?**
A) `AutoCompletion = true`
B) `CascadeCompletion = true`
C) `PropagateCompletion = true`
D) `ForwardCompletion = true`

**5. Which of the following is a "Source" block (primarily a producer)?**
A) `ActionBlock<T>`
B) `TransformManyBlock<TInput, TOutput>`
C) `BatchBlock<T>`
D) `BufferBlock<T>`

**6. You have a pipeline that downloads an image, resizes it, and saves it. The download step is slow. How can you improve performance?**
A) Use a single `ActionBlock` for all three steps.
B) Set `MaxDegreeOfParallelism` > 1 on the download block.
C) Decrease the `BoundedCapacity` of the resize block.
D) Link the blocks without `PropagateCompletion`.

**7. What is "backpressure" in the context of TPL Dataflow?**
A) The process of forcing data through a pipeline faster.
B) A mechanism where downstream blocks can slow down upstream blocks to prevent being overwhelmed.
C) An error condition where a block rejects all incoming messages.
D) The act of manually pulling data from a source block.

**8. Which block is best suited for splitting one incoming item into multiple outgoing items?**
A) `TransformBlock<TInput, TOutput>`
B) `BatchBlock<T>`
C) `TransformManyBlock<TInput, TOutput>`
D) `BroadcastBlock<T>`
