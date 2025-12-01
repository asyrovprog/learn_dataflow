using System;
using System.IO;
using System.Threading.Tasks;
using Lab.Iter09;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Lab 09: Cancellable File Processor ===\n");
        
        // Setup test files
        SetupTestFiles();
        
        int passed = 0;
        int total = 4;
        
        // Test 1: Process all files without cancellation (graceful shutdown)
        Console.WriteLine("[Test 1] Process all files without cancellation");
        try
        {
            var processor1 = new FileProcessor();
            processor1.Post("test1.txt");
            processor1.Post("test2.txt");
            processor1.Post("test3.txt");
            
            await Task.Delay(200); // Let files process
            
            var graceful = await processor1.ShutdownAsync();
            var stats = processor1.GetStats();
            
            if (stats.Processed == 3 && stats.Skipped == 0 && stats.Failed == 0 && graceful)
            {
                Console.WriteLine("✓ PASS: All 3 files processed, graceful shutdown\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected (3 processed, 0 skipped, 0 failed, graceful=true), got ({stats.Processed} processed, {stats.Skipped} skipped, {stats.Failed} failed, graceful={graceful})");
                Console.WriteLine("→ Check TODO[N1] (Interlocked counters) and TODO[N2] (graceful shutdown)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1], TODO[N2], and TODO[N3]\n");
        }
        
        // Test 2: Cancel immediately (no files processed)
        Console.WriteLine("[Test 2] Cancel immediately before processing");
        try
        {
            var processor2 = new FileProcessor();
            processor2.Post("test1.txt");
            processor2.Post("test2.txt");
            processor2.Post("test3.txt");
            
            processor2.Cancel(); // Immediate cancellation
            
            var graceful = await processor2.ShutdownAsync();
            var stats = processor2.GetStats();
            
            if (stats.Processed == 0 && stats.Total >= 0 && !graceful)
            {
                Console.WriteLine($"✓ PASS: 0 processed, {stats.Skipped} skipped, forced shutdown\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected (0 processed, forced=false), got ({stats.Processed} processed, forced={graceful})");
                Console.WriteLine("→ Check TODO[N1] (IsCancellationRequested check) and TODO[N3] (token passing)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1] and TODO[N3]\n");
        }
        
        // Test 3: Cancel after partial processing
        Console.WriteLine("[Test 3] Cancel after 100ms (partial processing)");
        try
        {
            var processor3 = new FileProcessor();
            for (int i = 1; i <= 10; i++)
            {
                processor3.Post($"test{i % 3 + 1}.txt");
            }
            
            await Task.Delay(100); // Let some process
            processor3.Cancel();
            
            var graceful = await processor3.ShutdownAsync();
            var stats = processor3.GetStats();
            
            // After cancellation, some items are processed, some skipped, some never dequeued
            if (stats.Processed > 0 && stats.Processed < 10 && !graceful)
            {
                Console.WriteLine($"✓ PASS: Partial processing ({stats.Processed} processed, {stats.Skipped} skipped, forced shutdown)\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected (partial processing, forced=false), got ({stats.Processed} processed, {stats.Total} total, forced={graceful})");
                Console.WriteLine("→ Check TODO[N1] (counter accuracy) and TODO[N2] (timeout handling)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check all TODOs\n");
        }
        
        // Test 4: Thread safety verification
        Console.WriteLine("[Test 4] Thread safety with parallel processing");
        try
        {
            var processor4 = new FileProcessor();
            for (int i = 0; i < 30; i++)
            {
                processor4.Post($"test{i % 3 + 1}.txt");
            }
            
            await Task.Delay(600); // Let all process
            await processor4.ShutdownAsync();
            var stats = processor4.GetStats();
            
            if (stats.Total == 30)
            {
                Console.WriteLine($"✓ PASS: Thread-safe counters (30/30 accounted)\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Lost counts! Expected 30 total, got {stats.Total}");
                Console.WriteLine("→ Check TODO[N1] - must use Interlocked.Increment, not regular ++\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1] (Interlocked operations)\n");
        }
        
        // Cleanup
        CleanupTestFiles();
        
        // Summary
        Console.WriteLine($"========================");
        Console.WriteLine($"Final Score: {passed}/{total} tests passed");
        Console.WriteLine($"========================");
        
        if (passed == total)
        {
            Console.WriteLine("🎉 All tests passed! Lab complete.");
        }
        else
        {
            Console.WriteLine($"⚠ {total - passed} test(s) failed. Review TODOs in README.md");
        }
    }
    
    static void SetupTestFiles()
    {
        File.WriteAllText("test1.txt", "Hello world from test file one");
        File.WriteAllText("test2.txt", "Sample text for processing in file two");
        File.WriteAllText("test3.txt", "Third test file with some content here");
    }
    
    static void CleanupTestFiles()
    {
        try
        {
            File.Delete("test1.txt");
            File.Delete("test2.txt");
            File.Delete("test3.txt");
        }
        catch { }
    }
}
