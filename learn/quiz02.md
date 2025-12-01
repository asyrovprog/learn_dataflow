# BatchBlock & Grouping Data Quiz

**1. What type does `BatchBlock<int>` output when a batch is ready?**
A) `int`
B) `List<int>`
C) `int[]`
D) `IEnumerable<int>`

**2. You have a `BatchBlock<string>(10)`. After posting 7 items and calling `Complete()`, what happens?**
A) The 7 items are lost
B) An exception is thrown
C) The block waits indefinitely for 3 more items
D) You must call `TriggerBatch()` to output the incomplete batch

**3. What is the default mode for BatchBlock?**
A) Non-greedy
B) Greedy
C) Lazy
D) Eager

**4. In greedy mode, when does BatchBlock accept messages?**
A) Only when it has exactly `batchSize` available slots
B) Immediately as they arrive
C) Only after all linked sources have provided messages
D) After a timeout period

**5. What is a key benefit of batching database inserts?**
A) Improved data accuracy
B) Reduced number of transactions/round-trips
C) Automatic rollback on errors
D) Better data compression

**6. You want batches to trigger every 5 seconds OR when 100 items accumulate (whichever comes first). Which component do you need besides BatchBlock?**
A) `BufferBlock`
B) `Timer` to call `TriggerBatch()`
C) `TransformBlock`
D) `BroadcastBlock`

**7. When should you avoid using BatchBlock with parallel processing?**
A) When processing large datasets
B) When order of processing matters critically
C) When batch size exceeds 1000 items
D) When using async/await

**8. What does `Greedy = false` enable in BatchBlock?**
A) Faster processing
B) Coordination across multiple sources before accepting any messages
C) Automatic batch size adjustment
D) Memory optimization
