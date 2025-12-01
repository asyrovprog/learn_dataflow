using System;
using System.Threading.Tasks;
using Lab.Iter06;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Processing 8 events...");
        
        var router = new EventRouter();
        router.BuildPipeline();

        // Post test events
        router.PostEvent(new Event("Security", "Critical", "Unauthorized access attempt"));
        router.PostEvent(new Event("Performance", "Warning", "High memory usage"));
        router.PostEvent(new Event("Application", "Info", "User logged in"));
        router.PostEvent(new Event("Performance", "Critical", "Service timeout"));
        router.PostEvent(new Event("Application", "Warning", "Cache miss"));
        router.PostEvent(new Event("Security", "Warning", "Multiple login attempts"));
        router.PostEvent(new Event("Security", "Info", "Password changed"));
        router.PostEvent(new Event("Application", "Critical", "Database connection lost"));

        // Signal completion
        router.CompleteSource();

        // Wait for pipeline to complete
        bool completed = await router.WaitForCompletion();

        Console.WriteLine();
        
        if (!completed)
        {
            Console.WriteLine("✗ Pipeline did not complete within timeout");
            return 1;
        }

        Console.WriteLine("✓ All events processed");
        Console.WriteLine("✓ Pipeline completed successfully");

        // Validate routing
        bool allTestsPassed = true;

        // Expected: 1 Security Critical event
        if (router.SecurityCount == 1)
        {
            Console.WriteLine($"✓ Security handler completed: {router.SecurityCount} events");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N1] - Security handler: expected 1, got {router.SecurityCount} - see README section 'TODO N1 – Implement Predicate-Based Routing Logic'");
            allTestsPassed = false;
        }

        // Expected: 2 Performance events (1 Critical + 1 Warning)
        if (router.PerformanceCount == 2)
        {
            Console.WriteLine($"✓ Performance handler completed: {router.PerformanceCount} events");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N1] - Performance handler: expected 2, got {router.PerformanceCount} - see README section 'TODO N1 – Implement Predicate-Based Routing Logic'");
            allTestsPassed = false;
        }

        // Expected: 5 General events (2 Security non-Critical + 2 Application non-Critical + 1 Application Critical routed here)
        // Actually: Security.Warning, Security.Info, Application.Info, Application.Warning, Application.Critical = 5
        if (router.GeneralCount == 5)
        {
            Console.WriteLine($"✓ General handler completed: {router.GeneralCount} events");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N1] - General handler: expected 5, got {router.GeneralCount} - see README section 'TODO N1 – Implement Predicate-Based Routing Logic'");
            allTestsPassed = false;
        }

        // Expected: 3 Critical events total (Security.Critical, Performance.Critical, Application.Critical)
        if (router.AuditCount == 3)
        {
            Console.WriteLine($"✓ Audit log completed: {router.AuditCount} critical events");
        }
        else
        {
            Console.WriteLine($"✗ FAIL TODO[N2] - Audit log: expected 3, got {router.AuditCount} - see README section 'TODO N2 – Build Complete Pipeline with Audit Tap'");
            allTestsPassed = false;
        }

        return allTestsPassed ? 0 : 1;
    }
}
