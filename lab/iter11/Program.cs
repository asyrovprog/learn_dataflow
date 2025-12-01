using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lab.Iter11;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Lab 11: Rate-Limited API Request Queue ===\n");

        int passed = 0;
        int total = 4;

        // Test 1: Bounded channel with backpressure
        try
        {
            var queue = new RateLimitedQueue(capacity: 10, maxRequestsPerSecond: 20);
            var cts = new CancellationTokenSource();
            
            var consumerTask = Task.Run(() => queue.ProcessRequestsAsync(cts.Token));
            
            for (int i = 0; i < 15; i++)
            {
                await queue.EnqueueAsync(new ApiRequest(i, $"/api/endpoint{i}"));
            }
            
            queue.Complete();
            await consumerTask;
            
            var (success, failed, throttled) = queue.GetStatistics();
            
            if (success + failed == 15 && throttled == 0)
            {
                Console.WriteLine("[Test 1] Bounded channel with capacity ✓ PASS");
                Console.WriteLine($"→ Processed {success + failed}/15 requests (Success: {success}, Failed: {failed})\n");
                passed++;
            }
            else
            {
                Console.WriteLine("[Test 1] ✗ FAIL: Expected 15 processed requests");
                Console.WriteLine($"→ Got Success={success}, Failed={failed}, Throttled={throttled}");
                Console.WriteLine("→ Check TODO[N1] (bounded channel creation)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test 1] ✗ FAIL: Exception - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1] and TODO[N3]\n");
        }

        // Test 2: Rate limiting (10 req/sec)
        try
        {
            var queue = new RateLimitedQueue(capacity: 50, maxRequestsPerSecond: 10);
            var cts = new CancellationTokenSource();
            
            var consumerTask = Task.Run(() => queue.ProcessRequestsAsync(cts.Token));
            var startTime = DateTime.UtcNow;
            
            for (int i = 0; i < 25; i++)
            {
                await queue.EnqueueAsync(new ApiRequest(i, $"/api/test{i}"));
            }
            
            queue.Complete();
            await consumerTask;
            
            var elapsed = DateTime.UtcNow - startTime;
            
            if (elapsed.TotalSeconds >= 2.0 && elapsed.TotalSeconds < 4.0)
            {
                Console.WriteLine("[Test 2] Rate limiting (10 req/sec) ✓ PASS");
                Console.WriteLine($"→ 25 requests processed in {elapsed.TotalSeconds:F1}s (expected ~2-3s)\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"[Test 2] ✗ FAIL: Expected ~2-3 seconds, got {elapsed.TotalSeconds:F1}s");
                Console.WriteLine("→ Check TODO[N2] (SemaphoreSlim initialization and usage)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test 2] ✗ FAIL: Exception - {ex.Message}");
            Console.WriteLine("→ Check TODO[N2]\n");
        }

        // Test 3: Statistics tracking
        try
        {
            var queue = new RateLimitedQueue(capacity: 50, maxRequestsPerSecond: 50);
            var cts = new CancellationTokenSource();
            
            var consumerTask = Task.Run(() => queue.ProcessRequestsAsync(cts.Token));
            
            for (int i = 0; i < 30; i++)
            {
                await queue.EnqueueAsync(new ApiRequest(i, $"/api/data{i}"));
            }
            
            queue.Complete();
            await consumerTask;
            
            var (success, failed, throttled) = queue.GetStatistics();
            
            if (success == 27 && failed == 3 && throttled == 0)
            {
                Console.WriteLine("[Test 3] Statistics tracking (thread-safe counters) ✓ PASS");
                Console.WriteLine($"→ Success: {success}, Failed: {failed}, Throttled: {throttled}\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"[Test 3] ✗ FAIL: Expected Success=27, Failed=3, Throttled=0");
                Console.WriteLine($"→ Got Success={success}, Failed={failed}, Throttled={throttled}");
                Console.WriteLine("→ Check TODO[N3] (Interlocked counter updates)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test 3] ✗ FAIL: Exception - {ex.Message}");
            Console.WriteLine("→ Check TODO[N3]\n");
        }

        // Test 4: Graceful cancellation
        try
        {
            var queue = new RateLimitedQueue(capacity: 20, maxRequestsPerSecond: 5);
            var cts = new CancellationTokenSource();
            
            var consumerTask = Task.Run(() => queue.ProcessRequestsAsync(cts.Token));
            
            for (int i = 0; i < 10; i++)
            {
                await queue.EnqueueAsync(new ApiRequest(i, $"/api/cancel{i}"));
            }
            
            cts.CancelAfter(TimeSpan.FromMilliseconds(500));
            queue.Complete();
            
            try
            {
                await consumerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            
            var (success, failed, throttled) = queue.GetStatistics();
            
            if (success + failed > 0 && success + failed < 10)
            {
                Console.WriteLine("[Test 4] Graceful cancellation with partial processing ✓ PASS");
                Console.WriteLine($"→ Partial processing: {success + failed}/10 requests before cancellation\n");
                passed++;
            }
            else
            {
                Console.WriteLine("[Test 4] ✗ FAIL: Expected partial processing (1-9 requests)");
                Console.WriteLine($"→ Got {success + failed} processed requests");
                Console.WriteLine("→ Check TODO[N3] (cancellation handling)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test 4] ✗ FAIL: Exception - {ex.Message}");
            Console.WriteLine("→ Check TODO[N3]\n");
        }

        // Summary
        Console.WriteLine("========================");
        Console.WriteLine($"Final Score: {passed}/{total} tests passed");
        Console.WriteLine("========================");

        if (passed == total)
        {
            Console.WriteLine("🎉 All tests passed! Lab complete.");
        }
        else
        {
            Console.WriteLine($"⚠ {total - passed} test(s) failed. Review TODOs in README.md");
        }
    }
}
