using System;
using System.Threading.Tasks;
using Lab.Iter07;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Coordinator: Creating 5 workers...");
        
        var coordinator = new WorkflowCoordinator();
        coordinator.CreateStartSignal();

        // Create worker tasks
        var workerTasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            int workerId = i + 1;
            workerTasks[i] = Task.Run(async () => 
                await coordinator.RunWorkerAsync(workerId, 100));
        }

        // Give workers time to start waiting
        await Task.Delay(200);

        Console.WriteLine("\nCoordinator: Broadcasting start signal...");
        bool firstBroadcast = coordinator.BroadcastStartSignal();
        
        // Try broadcasting again (should be rejected)
        bool secondBroadcast = coordinator.BroadcastStartSignal();

        // Wait for all workers
        bool allCompleted = await coordinator.WaitForAllWorkersAsync(workerTasks, 5);

        Console.WriteLine();

        // Validate results
        bool allTestsPassed = true;

        // Test 1: All workers started
        if (coordinator.StartedCount == 5)
        {
            Console.WriteLine("✓ All workers started");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N2] - Expected 5 workers to start, got {coordinator.StartedCount} - see README section 'TODO N2 – Implement Worker Task Logic'");
            allTestsPassed = false;
        }

        // Test 2: All workers completed
        if (coordinator.CompletedCount == 5)
        {
            Console.WriteLine("✓ All workers completed");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N2] - Expected 5 workers to complete, got {coordinator.CompletedCount} - see README section 'TODO N2 – Implement Worker Task Logic'");
            allTestsPassed = false;
        }

        // Test 3: Coordination completed successfully
        if (allCompleted)
        {
            Console.WriteLine("✓ All workers coordinated successfully");
        }
        else
        {
            Console.WriteLine("✗ FAIL TODO[N3] - Worker coordination failed or timed out - see README section 'TODO N3 – Verify Coordination & Completion'");
            allTestsPassed = false;
        }

        // Test 4: First broadcast succeeded
        if (firstBroadcast)
        {
            Console.WriteLine("✓ Signal broadcast successful (returned true on first attempt)");
        }
        else
        {
            Console.WriteLine("✗ FAIL TODO[N1] - First broadcast should return true - see README section 'TODO N1 – Implement Start Signal Block'");
            allTestsPassed = false;
        }

        // Test 5: Second broadcast rejected
        if (!secondBroadcast)
        {
            Console.WriteLine("✓ Second broadcast attempt rejected (returned false)");
        }
        else
        {
            Console.WriteLine("✗ FAIL TODO[N1] - Second broadcast should return false (WriteOnceBlock accepts only one value) - see README section 'TODO N1 – Implement Start Signal Block'");
            allTestsPassed = false;
        }

        return allTestsPassed ? 0 : 1;
    }
}
