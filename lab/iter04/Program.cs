using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lab.Iter04;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Lab 04: Dual Path Fork/Join Custom Block\n");

        bool allPassed = true;

        // TEST 1: Internal pipeline produces expected output for single item
        Console.WriteLine("TEST 1: Internal pipeline assembly (TODO N1)");
        try
        {
            var (input, output) = DualPathForkJoin.BuildInternalForkJoinPipeline();
            await input.SendAsync("Hello");
            input.Complete();
            var receivable1 = output as IReceivableSourceBlock<ProcessedItem>;
            if (receivable1 != null)
            {
                while (receivable1.TryReceive(out var item))
                {
                    if (item.LengthScore != 5 || item.VowelCount != 2)
                    {
                        Console.WriteLine("  FAIL: TODO[N1] not satisfied – see README section 'TODO N1 – Build Fork/Join Internal Pipeline'");
                        allPassed = false;
                    }
                }
            }
            if (allPassed) Console.WriteLine("  PASS\n");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  FAIL: TODO[N1] not satisfied – see README section 'TODO N1 – Build Fork/Join Internal Pipeline'\n");
            allPassed = false;
        }

        // TEST 2: Encapsulated block processes multiple items (TODO N2)
        Console.WriteLine("TEST 2: Encapsulated block (TODO N2)");
        try
        {
            var block = DualPathForkJoin.CreateForkJoinBlock();
            string[] inputs = { "Hello", "Dataflow", "TPL", "Encapsulation" };
            foreach (var s in inputs)
            {
                await block.SendAsync(s);
            }
            block.Complete();

            var source = (ISourceBlock<ProcessedItem>)block;
            var received = new List<ProcessedItem>();
            var receivable2 = source as IReceivableSourceBlock<ProcessedItem>;
            while (await source.OutputAvailableAsync())
            {
                if (receivable2 != null && receivable2.TryReceive(out var item))
                {
                    received.Add(item);
                }
            }
            await block.Completion; // ensure all processed

            if (received.Count != inputs.Length)
            {
                Console.WriteLine("  FAIL: TODO[N2] not satisfied – see README section 'TODO N2 – Encapsulate Composite as Custom Block'");
                allPassed = false;
            }
            else
            {
                bool metricsOk = 
                    received.Exists(r => r.Content == "Hello" && r.LengthScore == 5 && r.VowelCount == 2) &&
                    received.Exists(r => r.Content == "Dataflow" && r.LengthScore == 8 && r.VowelCount == 3) &&
                    received.Exists(r => r.Content == "TPL" && r.LengthScore == 3 && r.VowelCount == 0) &&
                    received.Exists(r => r.Content == "Encapsulation" && r.LengthScore == 13 && r.VowelCount == 6);

                if (!metricsOk)
                {
                    Console.WriteLine("  FAIL: TODO[N2] not satisfied – see README section 'TODO N2 – Encapsulate Composite as Custom Block'");
                    allPassed = false;
                }
            }

            if (allPassed) Console.WriteLine("  PASS\n");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  FAIL: TODO[N2] not satisfied – see README section 'TODO N2 – Encapsulate Composite as Custom Block'\n");
            allPassed = false;
        }

        // TEST 3 (Optional): Diagnostics (TODO N3)
        Console.WriteLine("TEST 3: Diagnostics (TODO N3 – optional)");
        try
        {
            var stats = DualPathForkJoin.GetDiagnostics();
            if (stats.Total < 4)
            {
                Console.WriteLine("  FAIL: TODO[N3] not satisfied – see README section 'TODO N3 – Add Diagnostics (Optional)'");
                allPassed = false;
            }
            else
            {
                Console.WriteLine("  PASS (optional)\n");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("  SKIP: TODO[N3] not implemented (optional)\n");
        }

        Console.WriteLine("\n" + (allPassed ? "ALL TESTS PASSED" : "SOME TESTS FAILED"));
    }
}
