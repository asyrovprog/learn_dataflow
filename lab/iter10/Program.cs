using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Iter10;
using System.Threading.Tasks.Dataflow;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Lab 10: Priority Buffer Block ===\n");

        int passed = 0;
        int total = 4;

        // Test 1: Priority ordering
        Console.WriteLine("[Test 1] Priority ordering - highest priority first");
        try
        {
            var block = new PriorityBufferBlock<string>();
            var received = new List<string>();
            var action = new ActionBlock<string>(x => received.Add(x));

            block.LinkTo(action, new DataflowLinkOptions { PropagateCompletion = true });

            block.Post((Priority: 1, Value: "Low"));
            block.Post((Priority: 5, Value: "High"));
            block.Post((Priority: 3, Value: "Medium"));

            block.Complete();
            await block.Completion;
            await action.Completion;

            if (received.Count == 3 && 
                received[0] == "High" && 
                received[1] == "Medium" && 
                received[2] == "Low")
            {
                Console.WriteLine("✓ PASS: Messages ordered by priority (5→3→1)\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected [High, Medium, Low], got [{string.Join(", ", received)}]");
                Console.WriteLine("→ Check TODO[N1] (priority storage) and TODO[N2] (propagation order)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1] and TODO[N2]\n");
        }

        // Test 2: FIFO within same priority
        Console.WriteLine("[Test 2] FIFO ordering within same priority");
        try
        {
            var block = new PriorityBufferBlock<int>();
            var received = new List<int>();
            var action = new ActionBlock<int>(x => received.Add(x));

            block.LinkTo(action, new DataflowLinkOptions { PropagateCompletion = true });

            block.Post((Priority: 2, Value: 10));
            block.Post((Priority: 2, Value: 20));
            block.Post((Priority: 2, Value: 30));

            block.Complete();
            await block.Completion;
            await action.Completion;

            if (received.Count == 3 && received[0] == 10 && received[1] == 20 && received[2] == 30)
            {
                Console.WriteLine("✓ PASS: FIFO maintained within same priority\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected [10, 20, 30], got [{string.Join(", ", received)}]");
                Console.WriteLine("→ Check TODO[N1] - sequence number for FIFO within priority\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1]\n");
        }

        // Test 3: Proper completion (drain buffer)
        Console.WriteLine("[Test 3] Completion after draining buffer");
        try
        {
            var block = new PriorityBufferBlock<string>();
            var received = new List<string>();
            var action = new ActionBlock<string>(x => { received.Add(x); });

            block.LinkTo(action, new DataflowLinkOptions { PropagateCompletion = true });

            block.Post((Priority: 1, Value: "A"));
            block.Post((Priority: 2, Value: "B"));
            block.Post((Priority: 3, Value: "C"));

            block.Complete();
            
            await block.Completion;
            await action.Completion;

            if (received.Count == 3 && block.Completion.IsCompletedSuccessfully)
            {
                Console.WriteLine("✓ PASS: Block completed after draining all messages\n");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ FAIL: Expected 3 messages and successful completion");
                Console.WriteLine("→ Check TODO[N3] (CheckCompletion must wait for buffer to drain)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N3]\n");
        }

        // Test 4: Declining permanently after Complete()
        Console.WriteLine("[Test 4] Declining permanently after Complete()");
        try
        {
            var block = new PriorityBufferBlock<int>();
            var action = new ActionBlock<int>(_ => { });

            block.LinkTo(action);
            block.Complete();

            await Task.Delay(10); // Let Complete() process

            bool accepted = block.Post((Priority: 1, Value: 999));

            await block.Completion;

            if (!accepted)
            {
                Console.WriteLine("✓ PASS: Post() returned false after Complete()\n");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: Post() should return false after Complete()");
                Console.WriteLine("→ Check TODO[N1] (return DecliningPermanently when _decliningPermanently=true)\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Exception thrown - {ex.Message}");
            Console.WriteLine("→ Check TODO[N1] and TODO[N3]\n");
        }

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
}
