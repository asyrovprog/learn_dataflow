# Quiz 11: Producer-Consumer Patterns Beyond Dataflow

## Q1. Which statement about Channel architecture is TRUE? (Single: A-D)

A) `ChannelWriter<T>` and `ChannelReader<T>` share the same internal queue  
B) Multiple readers automatically get broadcast copies of each item  
C) Channels require explicit thread synchronization when writing  
D) `ChannelReader<T>` provides both sync and async read methods

---

## Q2. When using `BoundedChannelFullMode.DropOldest` with capacity=3, what happens when queue `[1,2,3]` receives item `4`? (Single: A-D)

A) Item 4 is rejected, queue remains `[1,2,3]`  
B) Item 2 is removed, queue becomes `[1,3,4]`  
C) Item 1 is removed, queue becomes `[2,3,4]`  
D) Item 3 is removed, queue becomes `[1,2,4]`

---

## Q3. What is the PRIMARY difference between Channels and TPL Dataflow? (Single: A-D)

A) Channels support backpressure, Dataflow does not  
B) Dataflow is faster for simple producer-consumer scenarios  
C) Channels focus on storage, Dataflow combines storage + processing  
D) Channels provide built-in transformation blocks

---

## Q4. Which reading pattern is BEST for consuming all items until channel completes? (Single: A-D)

A) `while (channel.Reader.TryRead(out var item))`  
B) `await channel.Reader.ReadAsync()`  
C) `await foreach (var item in channel.Reader.ReadAllAsync())`  
D) `while (await channel.Reader.WaitToReadAsync())`

---

## Q5. What happens if you NEVER call `channel.Writer.Complete()`? (Multiple: A-D)

A) Consumer reading with `ReadAllAsync()` hangs forever  
B) Channel automatically completes after 30 seconds  
C) `WaitToReadAsync()` will never return false  
D) Memory leak if producer keeps writing to unbounded channel

---

## Q6. When should you use `TryWrite()` instead of `WriteAsync()`? (Multiple: A-D)

A) When you want non-blocking behavior  
B) When bounded channel might be full  
C) To avoid async overhead in hot paths  
D) When you need guaranteed delivery

---

## Q7. Benchmark shows Channels are ~10x faster than BufferBlock for simple producer-consumer. Why? (Single: A-D)

A) Channels use lock-free algorithms, Dataflow uses locks  
B) Channels focus on storage only, less overhead than Dataflow's processing pipeline  
C) Dataflow blocks allocate more memory per item  
D) Channels run on dedicated ThreadPool threads

---

## Q8. What optimization does `SingleReader = true` provide? (Single: A-D)

A) Guarantees only one consumer can read  
B) Enables faster internal implementation without multi-reader synchronization  
C) Automatically batches reads for better throughput  
D) Prevents concurrent `TryRead()` calls

---

## Q9. In a multi-producer scenario with 3 producers, how do you coordinate channel completion? (Single: A-D)

A) Each producer calls `Complete()` when done  
B) Use `Interlocked.Decrement()` on counter, last producer calls `Complete()`  
C) Call `Complete()` before starting producers  
D) Channels auto-complete when all producer tasks finish

---

## Q10. Why use bounded channels over unbounded? (Multiple: A-D)

A) Natural backpressure prevents memory exhaustion  
B) Bounded channels are always faster  
C) Signals to producers when consumers can't keep up  
D) Unbounded channels don't support completion

---

**Submit your answers in format:** `1:A,2:C,3:C,4:C,5:ACD,6:ABC,7:B,8:B,9:B,10:AC`
