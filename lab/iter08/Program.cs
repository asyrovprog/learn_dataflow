using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lab.Iter08;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Processing 8 orders...\n");

        var stats = new ProcessorStats();
        var processor = OrderProcessor.CreateProcessor(stats);

        // Collect results
        var results = new System.Collections.Generic.List<Result<Order>>();
        var collector = new ActionBlock<Result<Order>>(result => results.Add(result));
        
        processor.LinkTo(collector, new DataflowLinkOptions { PropagateCompletion = true });

        // Post test orders
        processor.Post(new Order("Alice", "alice@example.com", 1500, true));   // Express, discount
        processor.Post(new Order("Bob", "bob@example.com", 500, false));       // Standard
        processor.Post(new Order("Invalid", "invalid@example.com", -100, false)); // FAIL: Invalid amount
        processor.Post(new Order("Carol", "carol@example.com", 2000, true));   // Express, discount
        processor.Post(new Order("BadEmail", "bademail.com", 300, false));     // FAIL: Invalid email
        processor.Post(new Order("Dave", "dave@example.com", 800, false));     // Standard
        processor.Post(new Order("Eve", "eve@example.com", 1200, true));       // Express, discount
        processor.Post(new Order("Frank", "frank@example.com", 400, false));   // Standard

        processor.Complete();
        await collector.Completion;

        // Display results
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                var order = result.Value!;
                var fulfillment = order.IsExpress ? "Express" : "Standard";
                Console.WriteLine($"✓ Order ({order.CustomerName}): SUCCESS - {fulfillment} fulfillment");
            }
            else
            {
                Console.WriteLine($"✗ Order: FAILED - {result.ErrorMessage}");
            }
        }

        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($"- Total processed: {stats.TotalProcessed}");
        Console.WriteLine($"- Successful: {stats.SuccessCount}");
        Console.WriteLine($"- Failed: {stats.FailureCount}");
        Console.WriteLine($"- Express: {stats.ExpressCount}");
        Console.WriteLine($"- Standard: {stats.StandardCount}");

        Console.WriteLine();

        // Validate results
        bool allTestsPassed = true;

        // Test 1: All orders processed
        if (results.Count == 8)
        {
            Console.WriteLine("✓ All orders processed");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N2] - Expected 8 results, got {results.Count} - see README section 'TODO N2 – Build Encapsulated Pipeline'");
            allTestsPassed = false;
        }

        // Test 2: Pipeline completed without faulting
        if (processor.Completion.IsCompletedSuccessfully)
        {
            Console.WriteLine("✓ Pipeline completed without faulting");
        }
        else
        {
            Console.WriteLine("✗ FAIL TODO[N1] - Pipeline faulted (validation should use Result<T> not throw exceptions) - see README section 'TODO N1 – Implement Result Type and Validation'");
            allTestsPassed = false;
        }

        // Test 3: Correct success/failure counts
        if (stats.SuccessCount == 6 && stats.FailureCount == 2)
        {
            Console.WriteLine("✓ Correct success/failure counts");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N1]/[N3] - Expected 6 success/2 failures, got {stats.SuccessCount}/{stats.FailureCount} - see README sections 'TODO N1' and 'TODO N3'");
            allTestsPassed = false;
        }

        // Test 4: Correct routing counts
        if (stats.ExpressCount == 3 && stats.StandardCount == 3)
        {
            Console.WriteLine("✓ Correct routing counts");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N2]/[N3] - Expected 3 express/3 standard, got {stats.ExpressCount}/{stats.StandardCount} - see README sections 'TODO N2' and 'TODO N3'");
            allTestsPassed = false;
        }

        return allTestsPassed ? 0 : 1;
    }
}
