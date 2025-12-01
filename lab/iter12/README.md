# Lab 12 - Search Autocomplete with Rx.NET

## Overview
Build a search autocomplete feature using Reactive Extensions (Rx.NET) that demonstrates debouncing, async transformations, and proper subscription management.

## Scenario
You're building a search box that queries an API as the user types. To avoid overwhelming the API with requests for every keystroke, you need to:
- Debounce rapid keystrokes (wait 300ms after typing stops)
- Filter out empty/whitespace queries
- Transform queries to async API calls
- Handle results and manage subscriptions properly

## TODO N1 – Create Debounced Search Observable

**Objective:** Configure an observable pipeline that debounces keystrokes and filters invalid input.

**Requirements:**
- Start with `_searchSubject` as your source observable
- Apply `Throttle` operator with 300ms delay to debounce rapid keystrokes
- Chain `Where` operator to filter out `null`, empty, or whitespace-only strings
- Return the configured observable (don't call Subscribe yet)

**Key Concepts:**
- `Throttle` waits for a silence period before emitting the last value
- `Where` filters items based on a predicate (like LINQ Where)
- Operators are chainable and return new observables

**Hints:**
```csharp
return sourceObservable
    .Throttle(TimeSpan.FromMilliseconds(delay))
    .Where(predicate);
```

---

## TODO N2 – Transform to API Calls with SelectMany

**Objective:** Transform debounced queries into async API call observables using SelectMany.

**Requirements:**
- Take the debounced observable as input
- Use `SelectMany` to project each query string to an observable
- Convert `SearchApiAsync(query)` Task to observable using `Observable.FromAsync`
- Return the flattened observable that emits API results

**Key Concepts:**
- `SelectMany` flattens nested observables (like LINQ SelectMany or FP's flatMap)
- `Observable.FromAsync` converts a Task to an IObservable
- Each query triggers an async API call that's flattened into the result stream

**Hints:**
```csharp
return debouncedObservable
    .SelectMany(item => Observable.FromAsync(() => AsyncMethod(item)));
```

---

## TODO N3 – Subscribe and Handle Results

**Objective:** Subscribe to the API results observable and store results in `_apiCallLog`.

**Requirements:**
- Call `Subscribe()` on the API results observable
- In the OnNext handler, loop through the results array
- Add each result string to `_apiCallLog` list
- Return the `IDisposable` subscription for proper cleanup

**Key Concepts:**
- `Subscribe()` starts the observable execution
- OnNext handler receives each emitted value
- Returning IDisposable allows caller to unsubscribe/dispose
- Proper disposal prevents memory leaks

**Hints:**
```csharp
return observable.Subscribe(
    onNext: items => { /* process items */ },
    onError: ex => { /* optional */ },
    onCompleted: () => { /* optional */ }
);
```

---

## Expected Output

When all TODOs are complete, running `dotnet run` should show:

```
=== Lab Iter12: Search Autocomplete with Rx.NET ===

Test 1: Debouncing rapid keystrokes...
✅ PASS: Only final keystroke triggered API call

Test 2: Filtering empty strings...
✅ PASS: Empty/whitespace strings filtered out

Test 3: Multiple debounced searches...
✅ PASS: Multiple searches processed correctly

Test 4: Subscription disposal...
✅ PASS: Disposed before debounce completed, no API calls

✅ ALL TESTS PASSED!
```

---

## Running the Lab

```bash
cd lab/iter12
dotnet run
```

---

## Key Learning Points

After completing this lab, you'll understand:
- **Throttle operator**: Debouncing rapid events (user input, sensor data)
- **SelectMany**: Flattening async operations into observable streams
- **Observable.FromAsync**: Converting Tasks to observables
- **Subscription management**: Proper disposal to prevent memory leaks
- **Operator chaining**: Building complex pipelines from simple operators

---

## Hints

See `REF.md` for detailed hints and reference solution (open only after attempting).
