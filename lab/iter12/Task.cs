using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        // TODO[N1]: Create Debounced Search Observable
        // Create an observable that:
        // - Subscribes to _searchSubject
        // - Uses Throttle (300ms) to debounce rapid keystrokes
        // - Filters out empty/whitespace strings
        // - Returns the configured observable (don't subscribe yet)
        public IObservable<string> CreateDebouncedSearchObservable()
        {
            // [YOUR CODE GOES HERE]
            // Instructions:
            // 1. Start with _searchSubject as your source
            // 2. Apply Throttle operator with TimeSpan.FromMilliseconds(300)
            // 3. Chain Where operator to filter: !string.IsNullOrWhiteSpace(query)
            // 4. Return the configured observable
            return _searchSubject
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Where(e => !string.IsNullOrWhiteSpace(e));
        }

        // TODO[N2]: Transform to API Calls with SelectMany
        // Take the debounced observable and:
        // - Use SelectMany to transform each query to SearchApiAsync(query)
        // - Convert the Task<string[]> results to observable sequences
        // - Return the transformed observable
        public IObservable<string[]> TransformToApiCalls(IObservable<string> debouncedObservable)
        {
            // [YOUR CODE GOES HERE]
            // Instructions:
            // 1. Use SelectMany on debouncedObservable
            // 2. Inside SelectMany, convert query to observable: Observable.FromAsync(() => SearchApiAsync(query))
            // 3. Return the transformed observable
            return debouncedObservable.SelectMany(s => Observable.FromAsync(() => SearchApiAsync(s)));
        }

        // TODO[N3]: Subscribe and Handle Results
        // Subscribe to the API results observable:
        // - Store results in _apiCallLog (each result as separate entry)
        // - Return the subscription (IDisposable) for cleanup
        public IDisposable SubscribeToResults(IObservable<string[]> apiResultsObservable)
        {
            // [YOUR CODE GOES HERE]
            // Instructions:
            // 1. Call Subscribe() on apiResultsObservable
            // 2. In OnNext handler: loop through results array and add each to _apiCallLog
            // 3. Optional: Add OnError and OnCompleted handlers
            // 4. Return the IDisposable subscription
            var subscription = apiResultsObservable.Subscribe(onNext: (results) =>
            {
                foreach (var result in results)
                {
                    _apiCallLog.Add(result);
                }
            });
            return subscription;
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
