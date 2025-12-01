using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Lab.Iter11;

public record ApiRequest(int Id, string Endpoint);

public class RateLimitedQueue
{
    private readonly Channel<ApiRequest> _channel;
    private readonly SemaphoreSlim _rateLimiter;
    private int _successCount;
    private int _failedCount;
    private int _throttledCount;

    public RateLimitedQueue(int capacity, int maxRequestsPerSecond)
    {
        // TODO[N1]: Create bounded channel with backpressure
        // - Use Channel.CreateBounded<ApiRequest>() with BoundedChannelOptions
        // - Set capacity parameter
        // - Set FullMode = BoundedChannelFullMode.Wait (blocks producers when full)
        // - Set SingleReader = true (optimization - only one consumer)

        _channel = Channel.CreateBounded<ApiRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });

        // See README.md section "TODO N1: Create Bounded Channel"
        // [YOUR CODE GOES HERE]

        // TODO[N2]: Initialize rate limiter
        // - Create SemaphoreSlim with initial count = max count = maxRequestsPerSecond
        // - This controls how many requests can be processed concurrently
        _rateLimiter = new SemaphoreSlim(maxRequestsPerSecond);
    }

    public async Task<bool> EnqueueAsync(ApiRequest request, CancellationToken ct = default)
    {
        // TODO[N1]: Enqueue request with TryWrite → WriteAsync pattern
        // - Try TryWrite first (non-blocking fast path when queue has space)
        // - If TryWrite fails, use WriteAsync (async wait for space)
        // - Catch OperationCanceledException and increment _throttledCount with Interlocked
        // - Return true on success, false on cancellation
        if (!_channel.Writer.TryWrite(request))
        {
            try
            {
                await _channel.Writer.WriteAsync(request, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref _throttledCount);
                return false;
            }
        }
        return true;
    }

    public async Task ProcessRequestsAsync(CancellationToken ct)
    {
        // TODO[N3]: Consumer loop with rate limiting
        // - Wrap in try-catch for OperationCanceledException (expected on shutdown)
        // - Use await foreach with _channel.Reader.ReadAllAsync(ct)
        // - For each request:
        //   1. Acquire semaphore: await _rateLimiter.WaitAsync(ct)
        //   2. Fire-and-forget release after 1 second:
        //      _ = Task.Run(async () => { await Task.Delay(1000); _rateLimiter.Release(); });
        //   3. Process request: await ProcessRequestAsync(request)
        //   4. Update statistics with Interlocked:
        //      - Increment _successCount if true
        //      - Increment _failedCount if false
        int pending = 0;
        try
        {
            await foreach (var request in _channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                await _rateLimiter.WaitAsync(ct).ConfigureAwait(false);
                ReleaseAfterDelay();

                Task<bool> task;
                try
                {
                    Interlocked.Increment(ref pending);
                    task = ProcessRequestAsync(request);
                }
                catch (Exception)
                {
                    Interlocked.Decrement(ref pending);
                    Interlocked.Increment(ref _failedCount);
                    continue;
                }

                _ = task.ContinueWith(t =>
                {
                    if (t.IsFaulted || !t.Result)
                    {
                        Interlocked.Increment(ref _failedCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref _successCount);
                    }                   
                    Interlocked.Decrement(ref pending);

                }, TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (pending > 0)
        {
            await Task.Delay(100).ConfigureAwait(false);;
        }
    }

    private void ReleaseAfterDelay()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            _rateLimiter.Release();
        });
    }

    private async Task<bool> ProcessRequestAsync(ApiRequest request)
    {
        await Task.Delay(50);
        return request.Id % 10 != 0;  // Fail every 10th request
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }

    public (int Success, int Failed, int Throttled) GetStatistics()
    {
        return (_successCount, _failedCount, _throttledCount);
    }
}
