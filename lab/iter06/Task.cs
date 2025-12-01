using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter06
{
    public record Event(string Category, string Severity, string Message);

    public class EventRouter
    {
        private int _securityCount = 0;
        private int _performanceCount = 0;
        private int _generalCount = 0;
        private int _auditCount = 0;

        public int SecurityCount => _securityCount;
        public int PerformanceCount => _performanceCount;
        public int GeneralCount => _generalCount;
        public int AuditCount => _auditCount;

        // TODO[N1]: Implement Predicate-Based Routing Logic
        private Predicate<Event> SecurityCriticalPredicate => (e) => e.Category == "Security" && e.Severity == "Critical";
        private Predicate<Event> PerformancePredicate => (e) => e.Category == "Performance" && (e.Severity == "Critical" || e.Severity == "Warning");
        private Predicate<Event> GeneralPredicate => (e) => !(SecurityCriticalPredicate(e) || PerformancePredicate(e));
        private Predicate<Event> AuditPredicate => (e) => e.Severity == "Critical";

        private BufferBlock<Event>? _source;
        private ActionBlock<Event>? _securityHandler;
        private ActionBlock<Event>? _performanceHandler;
        private ActionBlock<Event>? _generalHandler;
        private ActionBlock<Event>? _auditLog;

        // TODO[N2]: Build Complete Pipeline with Audit Tap
        public void BuildPipeline()
        {
            var bo = new DataflowBlockOptions
            {
                BoundedCapacity = 256,
                EnsureOrdered = false
            };

            var eo = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                BoundedCapacity = 1024,
                EnsureOrdered = false
            };

            _source = new BufferBlock<Event>(bo);
            var broadcast = new BroadcastBlock<Event>(e => e, bo);
            _securityHandler = new ActionBlock<Event>(e => Interlocked.Increment(ref _securityCount), eo);
            _performanceHandler = new ActionBlock<Event>(e => Interlocked.Increment(ref _performanceCount), eo);
            _generalHandler = new ActionBlock<Event>(e => Interlocked.Increment(ref _generalCount), eo);
            _auditLog = new ActionBlock<Event>(e => Interlocked.Increment(ref _auditCount), eo);

            var lo = new DataflowLinkOptions { PropagateCompletion = true };
            _source.LinkTo(broadcast, lo);
            broadcast.LinkTo(_securityHandler, lo, SecurityCriticalPredicate);
            broadcast.LinkTo(_performanceHandler, lo, PerformancePredicate);
            broadcast.LinkTo(_generalHandler, lo, GeneralPredicate);
            broadcast.LinkTo(_auditLog, lo, AuditPredicate);
        }

        public void PostEvent(Event evt)
        {
            if (_source == null)
                throw new InvalidOperationException("Pipeline not built. Call BuildPipeline() first.");
            
            _source.Post(evt);
        }

        public void CompleteSource()
        {
            _source?.Complete();
        }

        // TODO[N3]: Verify Pipeline Completion
        public async Task<bool> WaitForCompletion()
        {
            // [YOUR CODE GOES HERE]
            if (_securityHandler == null || _performanceHandler == null ||
                _generalHandler == null || _auditLog == null)
            {
                return false;
            }

            var completed = Task.WhenAll(
                _securityHandler.Completion,
                _performanceHandler.Completion,
                _generalHandler.Completion,
                _auditLog.Completion);

            var delay = Task.Delay(5000);
            var task = await Task.WhenAny(completed, delay);

            return task == completed;
        }
    }
}
