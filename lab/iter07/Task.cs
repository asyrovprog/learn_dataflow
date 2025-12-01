using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter07
{
    public class WorkflowCoordinator
    {
        private WriteOnceBlock<bool>? _startSignal;
        private int _startedCount = 0;
        private int _completedCount = 0;

        public int StartedCount => _startedCount;
        public int CompletedCount => _completedCount;

        public void CreateStartSignal()
        {
            _startSignal = new WriteOnceBlock<bool>(null,
                new DataflowBlockOptions
                {
                    BoundedCapacity = 256,
                    EnsureOrdered = false
                });
        }

        public bool BroadcastStartSignal()
        {
            return _startSignal.Post(true);
        }

        // TODO[N2]: Implement Worker Task Logic
        // [YOUR CODE GOES HERE]
        public async Task RunWorkerAsync(int workerId, int workDurationMs)
        {
            await _startSignal.ReceiveAsync();
            Interlocked.Increment(ref _startedCount);
            Console.WriteLine($"Worker {workerId}: Started! Performing work...");
            await Task.Delay(workDurationMs).ConfigureAwait(false);
            Interlocked.Increment(ref _completedCount);
            Console.WriteLine($"Worker {workerId}: Completed!");
        }

        // TODO[N3]: Verify Coordination & Completion
        // [YOUR CODE GOES HERE]
        public async Task<bool> WaitForAllWorkersAsync(Task[] workerTasks, int expectedWorkerCount)
        {
            var workers = Task.WhenAll(workerTasks);
            var delay = Task.Delay(10000);
            var outcome = await Task.WhenAny(workers, delay).ConfigureAwait(false);
            return outcome == workers;
        }
    }
}
