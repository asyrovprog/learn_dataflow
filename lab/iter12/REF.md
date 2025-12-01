# Lab 12 Reference Guide

## TODO N1 Hints – Create Debounced Search Observable

### What is Throttle?
`Throttle` (also called `Debounce` in some Rx implementations) waits for a period of silence before emitting the most recent value. Perfect for search boxes where you want to wait until the user stops typing.

### Pattern
```csharp
observable
    .Throttle(TimeSpan.FromMilliseconds(delay))
    .Where(predicate)
```

### Common Mistakes
- Forgetting to return the observable
- Using `Subscribe()` inside (this method should only configure, not subscribe)
- Wrong TimeSpan syntax (use `TimeSpan.FromMilliseconds(300)`)

---

## TODO N2 Hints – Transform to API Calls with SelectMany

### What is SelectMany?
`SelectMany` projects each element to an observable sequence and flattens the results. It's like LINQ's `SelectMany` or functional programming's `flatMap`.

### Converting Task to Observable
```csharp
Observable.FromAsync(() => MethodReturningTask())
```

### Pattern
```csharp
observable.SelectMany(item => 
    Observable.FromAsync(() => AsyncMethod(item))
)
```

### Why SelectMany?
- Flattens nested observables into a single stream
- Handles async operations naturally
- Each emitted value triggers a new async operation

---

## TODO N3 Hints – Subscribe and Handle Results

### Subscribe Overloads
```csharp
// Simple
observable.Subscribe(onNext)

// With error handling
observable.Subscribe(onNext, onError)

// With completion
observable.Subscribe(onNext, onError, onCompleted)
```

### Processing Arrays
When you receive `string[]`, loop through and process each item:
```csharp
results => {
    foreach (var item in results) {
        // process item
    }
}
```

### Why Return IDisposable?
Returning the subscription allows the caller to:
- Unsubscribe when done
- Prevent memory leaks
- Stop receiving events

---

## Common Patterns

### Search Autocomplete Pattern
```csharp
searchSubject
    .Throttle(TimeSpan.FromMilliseconds(300))  // Debounce
    .Where(q => !string.IsNullOrWhiteSpace(q))  // Filter
    .SelectMany(q => Observable.FromAsync(() => SearchAsync(q)))  // Transform
    .Subscribe(results => ProcessResults(results));  // Handle
```

### Disposal Pattern
```csharp
private IDisposable? _subscription;

public void Start() {
    _subscription = observable.Subscribe(...);
}

public void Stop() {
    _subscription?.Dispose();
}
```

---

## Testing Strategy

1. **Test debouncing**: Rapid keystrokes should only trigger one API call
2. **Test filtering**: Empty/whitespace should be filtered out
3. **Test multiple searches**: Each debounced query should work independently
4. **Test disposal**: Disposing should stop future emissions

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Lab.Iter12
{
    public class SearchAutocomplete
    {
        private readonly Subject<string> _searchSubject = new();
        private readonly List<string> _searchLog = new();
        private readonly List<string> _apiCallLog = new();
        private IDisposable? _subscription;

        public IObservable<string> CreateDebouncedSearchObservable()
        {
            return _searchSubject
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Where(query => !string.IsNullOrWhiteSpace(query));
        }

        public IObservable<string[]> TransformToApiCalls(IObservable<string> debouncedObservable)
        {
            return debouncedObservable
                .SelectMany(query => Observable.FromAsync(() => SearchApiAsync(query)));
        }

        public IDisposable SubscribeToResults(IObservable<string[]> apiResultsObservable)
        {
            return apiResultsObservable.Subscribe(
                results =>
                {
                    foreach (var result in results)
                    {
                        _apiCallLog.Add(result);
                    }
                },
                ex => Console.WriteLine($"Error: {ex.Message}"),
                () => Console.WriteLine("Search stream completed")
            );
        }

        public void StartSearchPipeline()
        {
            var debounced = CreateDebouncedSearchObservable();
            var apiCalls = TransformToApiCalls(debounced);
            _subscription = SubscribeToResults(apiCalls);
        }

        public void SimulateKeystroke(string text)
        {
            _searchSubject.OnNext(text);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _searchLog.Add(text);
            }
        }

        public void Complete()
        {
            _searchSubject.OnCompleted();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _searchSubject.Dispose();
        }

        public List<string> GetSearchLog() => _searchLog;
        public List<string> GetApiCallLog() => _apiCallLog;

        private async Task<string[]> SearchApiAsync(string query)
        {
            await Task.Delay(50);
            return new[] { $"Result1:{query}", $"Result2:{query}", $"Result3:{query}" };
        }
    }
}
```

</details>
