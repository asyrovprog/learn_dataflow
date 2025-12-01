using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lab.Iter03;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Lab 03: Resilient Data Import Pipeline\n");

        // Test data with various error conditions
        var testLines = new[]
        {
            "1,Alice,25,alice@example.com",           // Valid
            "2,Bob,30,bob@example.com",                // Valid
            "3,Charlie,invalid,charlie@example.com",   // Invalid age
            "4,,28,dave@example.com",                  // Missing name
            "5,Eve,22,",                               // Missing email
            "6,Frank,150,frank@example.com",           // Age out of range
            "7,Grace,22,grace.com",                    // Email missing @
            "8,Henry,35",                              // Wrong field count
            "9,Ivy,28,ivy@example.com"                 // Valid
        };

        bool allPassed = true;

        // TEST 1: ParseCsvLine validates correctly
        Console.WriteLine("TEST 1: ParseCsvLine validation");
        
        try
        {
            var result1 = DataImporter.ParseCsvLine("1,Alice,25,alice@example.com");
            if (!result1.IsSuccess || result1.Data?.Name != "Alice")
            {
                Console.WriteLine("  FAIL: TODO[T1] not satisfied - see README section 'TODO T1 – Parse and Validate CSV Lines'");
                allPassed = false;
            }

            var result2 = DataImporter.ParseCsvLine("3,Charlie,invalid,charlie@example.com");
            if (result2.IsSuccess || !result2.ErrorMessage!.Contains("Age"))
            {
                Console.WriteLine("  FAIL: TODO[T1] not satisfied - see README section 'TODO T1 – Parse and Validate CSV Lines'");
                allPassed = false;
            }

            var result3 = DataImporter.ParseCsvLine("4,,28,dave@example.com");
            if (result3.IsSuccess || !result3.ErrorMessage!.Contains("Name"))
            {
                Console.WriteLine("  FAIL: TODO[T1] not satisfied - see README section 'TODO T1 – Parse and Validate CSV Lines'");
                allPassed = false;
            }

            var result4 = DataImporter.ParseCsvLine("5,Eve,22,");
            if (result4.IsSuccess || !result4.ErrorMessage!.Contains("Email"))
            {
                Console.WriteLine("  FAIL: TODO[T1] not satisfied - see README section 'TODO T1 – Parse and Validate CSV Lines'");
                allPassed = false;
            }

            if (allPassed)
                Console.WriteLine("  PASS\n");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  FAIL: TODO[T1] not satisfied - see README section 'TODO T1 – Parse and Validate CSV Lines'\n");
            allPassed = false;
        }

        // TEST 2: Pipeline routes valid and invalid records correctly
        Console.WriteLine("TEST 2: Pipeline routing with predicates");
        
        ConcurrentBag<UserData> successData = new();
        ConcurrentBag<string> errorMessages = new();
        
        try
        {
            var (inputBlock, successData2, errorMessages2) = DataImporter.CreateImportPipeline();
            successData = successData2;
            errorMessages = errorMessages2;
            
            foreach (var line in testLines)
            {
                await inputBlock.SendAsync(line);
            }
            
            inputBlock.Complete();
            
            // Wait for completion
            await Task.Delay(500);

            int expectedSuccess = 3; // Lines 1, 2, 9
            int expectedErrors = 6;  // Lines 3, 4, 5, 6, 7, 8

            if (successData.Count != expectedSuccess)
            {
                Console.WriteLine($"  FAIL: TODO[T2] not satisfied - see README section 'TODO T2 – Build Error-Handling Pipeline'");
                allPassed = false;
            }

            if (errorMessages.Count != expectedErrors)
            {
                Console.WriteLine($"  FAIL: TODO[T2] not satisfied - see README section 'TODO T2 – Build Error-Handling Pipeline'");
                allPassed = false;
            }

            // Verify error messages are descriptive
            bool hasDescriptiveErrors = errorMessages.Any(e => e.Contains("Age") || e.Contains("Name") || e.Contains("Email"));
            if (!hasDescriptiveErrors)
            {
                Console.WriteLine("  FAIL: TODO[T2] not satisfied - see README section 'TODO T2 – Build Error-Handling Pipeline'");
                allPassed = false;
            }

            if (allPassed)
                Console.WriteLine("  PASS\n");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  FAIL: TODO[T2] not satisfied - see README section 'TODO T2 – Build Error-Handling Pipeline'\n");
            allPassed = false;
        }

        // TEST 3: Statistics reporting
        Console.WriteLine("TEST 3: Statistics reporting");
        
        try
        {
            DataImporter.ReportStatistics(
                testLines.Length,
                successData.Count,
                errorMessages.Count,
                errorMessages.ToList()
            );
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  FAIL: TODO[T3] not satisfied - see README section 'TODO T3 – Implement Statistics Reporting'");
            allPassed = false;
        }

        Console.WriteLine("\n" + (allPassed ? "ALL TESTS PASSED" : "SOME TESTS FAILED"));
    }
}
