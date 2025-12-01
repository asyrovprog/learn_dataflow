# Quiz 12 Retry Answer Key: Reactive Extensions (Rx.NET)

**Correct Answers:** 1:A 2:B 3:Y 4:B 5:A 6:ABC 7:Y 8:B

---

## Question 1: A
**Correct Answer:** A) Each subscription gets its own independent execution starting from the beginning

**Explanation:** This is the defining characteristic of cold observables. Like Netflix, each subscriber gets their own independent playback. If you subscribe to `Observable.Range(1, 5)` twice, each subscription creates a new producer and receives its own sequence 1,2,3,4,5 from the start. Compare to hot observables (like live TV) where all subscribers share the same broadcast.

---

## Question 2: B
**Correct Answer:** B) Both use push-based propagation with different focus areas

**Explanation:** This is the key insight! Both TPL Dataflow and Rx.NET use **push-based** models. When you `Post()` to a dataflow block, it pushes downstream. When an observable emits, it pushes to observers. The real differences are:
- **Rx.NET**: Event-driven, time-centric, rich operator composition
- **TPL Dataflow**: Pipeline-centric, explicit flow control, bounded capacity/backpressure

Neither is pull-based. Pull-based would be like `IEnumerable` where you call `MoveNext()` to request items.

---

## Question 3: Y
**Correct Answer:** Y (Yes)

**Explanation:** `FirstAsync()` returns a `Task<T>` that completes when the first value is emitted. You can await it directly:
```csharp
var firstValue = await observable.FirstAsync();
```
You can also use `observable.GetAwaiter()` which internally uses `FirstAsync()`. This bridges the async/await world with the Rx world.

---

## Question 4: B
**Correct Answer:** B) Throttle (or Debounce)

**Explanation:** `Throttle` (also called `Debounce` in some Rx implementations) emits a value only after a period of silence. Perfect for search boxes:
```csharp
searchBox.TextChanged
    .Select(e => e.Text)
    .Throttle(TimeSpan.FromMilliseconds(300))
    .Subscribe(text => PerformSearch(text));
```
- **Sample**: Emits most recent value at fixed intervals (periodic sampling)
- **Buffer/Window**: Collect items into groups, not for debouncing

---

## Question 5: A
**Correct Answer:** A) Concat processes sequentially (waits for each to complete); Merge processes concurrently (interleaves items)

**Explanation:** 
- **Concat**: Like a queue - wait for first observable to complete, then start second, preserving strict order
- **Merge**: Like merging highway lanes - interleave items from all observables as they arrive, no order guarantee

Think: `Concat` = sequential playlist, `Merge` = multiple live feeds mixed together.

---

## Question 6: ABC
**Correct Answer:** A, B, C

**Explanation:** All three are correct:
- **A**: Projects each element to an observable sequence ✅
- **B**: Flattens nested observables into single stream ✅
- **C**: Equivalent to LINQ's SelectMany/FP's flatMap ✅
- **D**: FALSE - SelectMany works with IObservable, not just IEnumerable

Example: `clicks.SelectMany(_ => Observable.Timer(1000))` projects each click to a 1-second timer observable, then flattens all those timers into a single stream.

---

## Question 7: Y
**Correct Answer:** Y (Yes)

**Explanation:** `Subject<T>` implements both `IObservable<T>` (can be subscribed to) and `IObserver<T>` (can receive values via `OnNext`). This makes it a hot observable - it multicasts to all current subscribers. When you call `subject.OnNext(value)`, all subscribers receive it simultaneously. Subjects are the bridge between imperative and reactive code.

---

## Question 8: B
**Correct Answer:** B) Event streams with time-based windowing and complex operator composition

**Explanation:** Rx.NET shines when you need:
- **Time-based operators**: Throttle, Debounce, Window, Buffer with time, Sample
- **Event-driven scenarios**: UI events, sensor data, real-time updates
- **Complex composition**: Zip, CombineLatest, SelectMany chains

Channels are better for:
- **A, C, D**: Simple producer-consumer, bounded queues, basic async coordination

Think: Rx for events and time, Channels for queues, Dataflow for pipelines.

---

## Scoring Guide

- **8/8 (100%)**: Expert level - ready for production Rx.NET
- **7/8 (87.5%)**: Strong - minor review recommended
- **6/8 (75%)**: Pass - solid understanding
- **<6/8 (<75%)**: Review materials and retry

---

**Key Concepts Reinforced:**
1. Cold observable unicast behavior
2. Push-based nature of both Dataflow and Rx
3. FirstAsync() for async/Rx bridging
4. Throttle/Debounce for user input scenarios
5. Concat (sequential) vs Merge (concurrent)
6. SelectMany projection and flattening
7. Subject as hot observable bridge
8. When to choose Rx vs Channels vs Dataflow
