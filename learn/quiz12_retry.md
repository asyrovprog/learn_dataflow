# Quiz 12 Retry: Reactive Extensions (Rx.NET)

**Topic:** Reactive Extensions (Rx.NET)  
**Questions:** 8  
**Format:** Y/N, A-D, Multi-select (M:)  
**Passing Score:** 70% (6/8 correct)

---

## Question 1 (A-D)
What happens when you subscribe to a cold observable multiple times?

A) Each subscription gets its own independent execution starting from the beginning  
B) All subscriptions share the same execution and receive values simultaneously  
C) Only the first subscription receives values, others are ignored  
D) An exception is thrown on the second subscription  

**Your Answer:** ____

---

## Question 2 (A-D)
How does TPL Dataflow differ from Rx.NET in terms of data flow?

A) Dataflow uses pull-based consumption; Rx uses push-based  
B) Both use push-based propagation with different focus areas  
C) Dataflow is synchronous; Rx is asynchronous  
D) Dataflow doesn't support backpressure; Rx does  

**Your Answer:** ____

---

## Question 3 (Y/N)
You can use `await observable.FirstAsync()` to convert an observable sequence to a Task that completes with the first value.

**Your Answer:** ____

---

## Question 4 (A-D)
Which Rx operator would you use to prevent processing every keystroke in a search box, only processing after the user stops typing for 300ms?

A) Sample  
B) Throttle (or Debounce)  
C) Buffer  
D) Window  

**Your Answer:** ____

---

## Question 5 (A-D)
What is the difference between `Concat` and `Merge` when combining observables?

A) Concat processes sequentially (waits for each to complete); Merge processes concurrently (interleaves items)  
B) Concat preserves order; Merge always reverses order  
C) Concat is for hot observables; Merge is for cold observables  
D) They are aliases for the same operation  

**Your Answer:** ____

---

## Question 6 (M:)
Which statements about `SelectMany` are true? (Select all that apply)

A) It projects each element to an observable sequence  
B) It flattens nested observables into a single stream  
C) It's equivalent to LINQ's SelectMany or FP's flatMap  
D) It only works with IEnumerable, not IObservable  

**Your Answer:** ____

---

## Question 7 (Y/N)
Subjects in Rx.NET are both IObservable and IObserver, making them hot observables that can multicast to multiple subscribers.

**Your Answer:** ____

---

## Question 8 (A-D)
When choosing between Rx.NET and System.Threading.Channels, which scenario favors Rx.NET?

A) High-throughput producer-consumer with simple FIFO processing  
B) Event streams with time-based windowing and complex operator composition  
C) Bounded queues with backpressure and flow control  
D) Simple async coordination without time-based operations  

**Your Answer:** ____

---

## Scoring
- 8/8 correct: 100% ✅ Perfect!
- 7/8 correct: 87.5% ✅ Pass
- 6/8 correct: 75% ✅ Pass
- <6 correct: <75% ❌ Review recommended

---

**Instructions:** Provide your answers in compact format (e.g., `1:A 2:B 3:Y 4:B 5:A 6:ABC 7:Y 8:B`)
