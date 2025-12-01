using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
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

    // TODO[N1]: Build Fork/Join Internal Pipeline
    public static (ITargetBlock<string> input, ISourceBlock<ProcessedItem> output) BuildInternalForkJoinPipeline()
    {
        /*
        Implement BuildInternalForkJoinPipeline() to create and link the internal blocks:
            A BufferBlock<string> for input buffering.
            A BroadcastBlock<string> to duplicate messages.
            Two TransformBlock<string,(string,int)> blocks:
            Length path: (content, content.Length)
            Vowel path: (content, CountVowels(content))
            A JoinBlock<(string,int),(string,int)> to pair results from both paths.
            A final TransformBlock<Tuple<(string,int),(string,int)>, ProcessedItem> assembling the ProcessedItem.
        Requirements:
            Use EnsureOrdered = true to keep proper pairing.
            Propagate completion from input through each link.
            Return the input target and final output source blocks in a tuple.
        */

        var input = new BufferBlock<string>(new DataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = false,
        });

        var fork = new BroadcastBlock<string>((s) => s, new DataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = false,
        });

        var length = new TransformBlock<string, (string, int)>((s) => (s, s.Length), new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = true,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        var vowels = new TransformBlock<string, (string, int)>((s) =>
        {
            var count = 0;
            foreach (var c in s)
            {
                if ("aeiouAEIOU".Contains(c)) count++;
            }
            return (s, count);
        }
        , new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = true,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        var join = new JoinBlock<(string, int), (string, int)>(new GroupingDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = false,
            Greedy = true,
        });

        var assemble = new TransformBlock<Tuple<(string, int), (string, int)>, ProcessedItem>((e) =>
        {
            if (e.Item1.Item1 != e.Item2.Item1) throw new InvalidProgramException();
            Interlocked.Increment(ref _total);
            Interlocked.Add(ref _sumLength, e.Item1.Item2);
            Interlocked.Add(ref _sumVowels, e.Item2.Item2);
            return new ProcessedItem(e.Item1.Item1, e.Item1.Item2, e.Item2.Item2);
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = false,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        input.LinkTo(fork, new DataflowLinkOptions { PropagateCompletion = true });
        fork.LinkTo(length, new DataflowLinkOptions { PropagateCompletion = true });
        fork.LinkTo(vowels, new DataflowLinkOptions { PropagateCompletion = true });
        length.LinkTo(join.Target1, new DataflowLinkOptions { PropagateCompletion = true });
        vowels.LinkTo(join.Target2, new DataflowLinkOptions { PropagateCompletion = true });
        join.LinkTo(assemble, new DataflowLinkOptions { PropagateCompletion = true });

        return (input, assemble);
    }

    // TODO[N2]: Encapsulate Composite as Custom Block
    public static IPropagatorBlock<string, ProcessedItem> CreateForkJoinBlock()
    {
        var (input, assemble) = BuildInternalForkJoinPipeline();
        return DataflowBlock.Encapsulate(input, assemble);
    }

    // TODO[N3]: Diagnostics (Optional)
    public static ForkJoinDiagnostics GetDiagnostics()
    {
        int total = _total;
        double avgLength = total == 0 ? 0 : (double)_sumLength / total;
        double avgVowels = total == 0 ? 0 : (double)_sumVowels / total;
        return new ForkJoinDiagnostics(total, avgLength, avgVowels);
    }
}
