# Reference & Hints

## Hint: Counting Vowels
Treat vowels as `aeiou` case-insensitive. LINQ: `s.Count(c => "aeiouAEIOU".Contains(c))`.

## Hint: Diagnostics Aggregation
Use `Interlocked` for thread-safe counters in output block delegate.

## Hint: Fault Propagation
Wrap transform delegates in try/catch; call `((IDataflowBlock)final).Fault(ex)` OR let exception bubble (Dataflow will fault the block) then coordinate fault signaling outward by faulting remaining blocks.

## Hint: Encapsulation Pattern
Create internal graph then `return DataflowBlock.Encapsulate(input, output);`.

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter04;

public record ProcessedItem(string Content, int LengthScore, int VowelCount);
public record ForkJoinDiagnostics(int Total, double AvgLength, double AvgVowels);

public static class DualPathForkJoin
{
    private static int _total;
    private static long _sumLength;
    private static long _sumVowels;

    private static int CountVowels(string s) => s.Count(c => "aeiouAEIOU".Contains(c));

    public static (ITargetBlock<string> input, ISourceBlock<ProcessedItem> output) BuildInternalForkJoinPipeline()
    {
        var input = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 1024 });
        var broadcast = new BroadcastBlock<string>(s => s);

        var execOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            EnsureOrdered = true,
            BoundedCapacity = 1024
        };

        var lengthBlock = new TransformBlock<string, (string,int)>(s => (s, s.Length), execOptions);
        var vowelBlock  = new TransformBlock<string, (string,int)>(s => (s, CountVowels(s)), execOptions);

        var join = new JoinBlock<(string,int),(string,int)>(new GroupingDataflowBlockOptions { Greedy = true });

        var output = new TransformBlock<Tuple<(string,int),(string,int)>, ProcessedItem>(t =>
        {
            var content = t.Item1.Item1; // same in both
            var length = t.Item1.Item2;
            var vowels = t.Item2.Item2;
            Interlocked.Increment(ref _total);
            Interlocked.Add(ref _sumLength, length);
            Interlocked.Add(ref _sumVowels, vowels);
            return new ProcessedItem(content, length, vowels);
        }, execOptions);

        // Linking with completion propagation
        input.LinkTo(broadcast, new DataflowLinkOptions { PropagateCompletion = true });
        broadcast.LinkTo(lengthBlock, new DataflowLinkOptions { PropagateCompletion = true });
        broadcast.LinkTo(vowelBlock,  new DataflowLinkOptions { PropagateCompletion = true });
        lengthBlock.LinkTo(join.Target1, new DataflowLinkOptions { PropagateCompletion = true });
        vowelBlock.LinkTo(join.Target2,  new DataflowLinkOptions { PropagateCompletion = true });
        join.LinkTo(output, new DataflowLinkOptions { PropagateCompletion = true });

        return (input, output);
    }

    public static IPropagatorBlock<string, ProcessedItem> CreateForkJoinBlock()
    {
        var (input, output) = BuildInternalForkJoinPipeline();
        return DataflowBlock.Encapsulate(input, output);
    }

    public static ForkJoinDiagnostics GetDiagnostics()
    {
        int total = _total;
        double avgLength = total == 0 ? 0 : (double)_sumLength / total;
        double avgVowels = total == 0 ? 0 : (double)_sumVowels / total;
        return new ForkJoinDiagnostics(total, avgLength, avgVowels);
    }
}
```

</details>
