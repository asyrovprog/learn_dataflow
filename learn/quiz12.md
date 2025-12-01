# Quiz 12: Reactive Extensions (Rx.NET)

**Topic:** Reactive Extensions (Rx.NET)  
**Questions:** 8  
**Format:** Y/N, A-D, Multi-select (M:)  
**Passing Score:** 70% (6/8 correct)

---

## Question 1 (A-D)
What is the key difference between a cold and hot observable?

A) Cold observables start producing values only when subscribed; hot observables produce values regardless of subscriptions  
B) Cold observables are thread-safe; hot observables are not  
C) Cold observables can be disposed; hot observables cannot  
D) Cold observables support backpressure; hot observables do not  

**Your Answer:** ____

---

## Question 2 (Y/N)
An `IObservable<T>` pushes data to subscribers, whereas TPL Dataflow blocks require explicit linking and pull-based consumption.

**Your Answer:** ____

---

## Question 3 (A-D)
Which operator would you use to convert an `IObservable<T>` to `Task<T>` to get the first emitted value?

A) `ToTask()`  
B) `FirstAsync()`  
C) `GetAwaiter()`  
D) `AsTask()`  

**Your Answer:** ____

---

## Question 4 (M:)
Which of the following are valid backpressure strategies in Rx.NET? (Select all that apply)

A) Sample - emit most recent value at intervals  
B) Throttle - emit only after silence period  
C) Buffer - collect items into batches  
D) Retry - retry on error  

**Your Answer:** ____

---

## Question 5 (A-D)
What does the `SelectMany` operator do?

A) Filters items based on a predicate  
B) Projects each item to a sequence and flattens the results  
C) Merges multiple observables into one  
D) Groups items by a key  

**Your Answer:** ____

---

## Question 6 (Y/N)
`Merge` preserves the order of items from source observables, while `Concat` processes observables sequentially.

**Your Answer:** ____

---

## Question 7 (A-D)
When should you prefer Rx.NET over TPL Dataflow?

A) When you need complex multi-stage pipelines with backpressure  
B) When you have event-driven data streams with time-based operations  
C) When you need bounded capacity and flow control  
D) When you need custom IPropagatorBlock implementations  

**Your Answer:** ____

---

## Question 8 (M:)
Which operators are useful for error handling in Rx? (Select all that apply)

A) Catch - handle errors and switch to fallback observable  
B) Retry - retry the sequence on error  
C) OnErrorResumeNext - continue with next observable on error  
D) Debounce - suppress rapid events  

**Your Answer:** ____

---

## Scoring
- 8/8 correct: 100% ✅ Perfect!
- 7/8 correct: 87.5% ✅ Pass
- 6/8 correct: 75% ✅ Pass
- <6 correct: <75% ❌ Retry recommended

---

**Instructions:** Provide your answers in compact format (e.g., `1:A 2:Y 3:B 4:ABC 5:B 6:N 7:B 8:ABC`)
