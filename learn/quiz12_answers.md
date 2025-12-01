# Quiz 12 Answer Key: Reactive Extensions (Rx.NET)

**Correct Answers:** 1:A 2:N 3:B 4:ABC 5:B 6:N 7:B 8:ABC

---

## Question 1: A
**Correct Answer:** A) Cold observables start producing values only when subscribed; hot observables produce values regardless of subscriptions

**Explanation:** Cold observables are like Netflix - each subscriber gets their own independent stream from the beginning (e.g., `Observable.Range`, `Observable.Create`). Hot observables are like live TV - they broadcast regardless of who's watching (e.g., mouse events, `Subject<T>`). Thread-safety and disposal are orthogonal concerns.

---

## Question 2: N
**Correct Answer:** N (No)

**Explanation:** While the first part is correct (IObservable pushes data), the second part is misleading. TPL Dataflow is actually push-based as well - when you post to a block, it pushes data downstream via links. The key difference is that Rx is event-driven and time-centric, while Dataflow is pipeline-centric with explicit flow control and backpressure.

---

## Question 3: B
**Correct Answer:** B) `FirstAsync()`

**Explanation:** `FirstAsync()` returns a `Task<T>` that completes with the first emitted value. You can also use `await observable.FirstAsync()` or `observable.GetAwaiter()` (which internally uses FirstAsync). `ToTask()` doesn't exist, and `AsTask()` is not a standard Rx operator.

---

## Question 4: ABC
**Correct Answer:** A, B, C

**Explanation:** 
- **Sample**: Emits the most recent value at fixed intervals (throttles by time)
- **Throttle**: Emits only after a period of silence (debounce)
- **Buffer**: Collects items into batches
- **Retry** is for error handling, not backpressure management

---

## Question 5: B
**Correct Answer:** B) Projects each item to a sequence and flattens the results

**Explanation:** `SelectMany` is the Rx equivalent of LINQ's `SelectMany` or FP's `flatMap`. It takes each item, transforms it into an observable sequence, and flattens all those sequences into a single observable stream. Example: `events.SelectMany(e => Observable.Timer(...))` projects each event to a delayed observable.

---

## Question 6: N
**Correct Answer:** N (No)

**Explanation:** It's the opposite! `Merge` does NOT preserve order - it interleaves items from multiple observables as they arrive. `Concat` processes observables sequentially, preserving order by waiting for each observable to complete before starting the next one. Think: Merge = parallel lanes merging, Concat = sequential queue.

---

## Question 7: B
**Correct Answer:** B) When you have event-driven data streams with time-based operations

**Explanation:** Rx.NET excels at:
- Event-driven scenarios (UI events, sensor data, real-time updates)
- Time-based operators (Throttle, Debounce, Window, Buffer with time)
- Composition of async event streams

TPL Dataflow is better for:
- Complex multi-stage pipelines with explicit flow control (A)
- Bounded capacity and backpressure (C)
- Custom block implementations (D)

---

## Question 8: ABC
**Correct Answer:** A, B, C

**Explanation:**
- **Catch**: Handle errors and switch to a fallback observable
- **Retry**: Retry the observable sequence on error (with optional count/delay)
- **OnErrorResumeNext**: Continue with the next observable when an error occurs
- **Debounce** (option D) is for throttling rapid events, not error handling

---

## Scoring Guide

- **8/8 (100%)**: Expert level - ready to build production Rx.NET applications
- **7/8 (87.5%)**: Strong understanding - minor clarifications needed
- **6/8 (75%)**: Pass - review key concepts (hot/cold, operators)
- **<6/8 (<75%)**: Needs review - revisit learning materials and retry

---

**Common Misconceptions Tested:**
1. Cold vs hot observable semantics
2. Push vs pull-based consumption models
3. Async/Rx integration patterns
4. Backpressure operators vs error handling
5. SelectMany flattening behavior
6. Merge vs Concat ordering guarantees
7. When to use Rx vs Dataflow vs Channels
8. Error handling operator categories
