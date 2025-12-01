# Lab 06 Reference Guide

## TODO N1 Hints – Predicate-Based Routing Logic

### Understanding the Routing Rules
You need to create predicate functions that return `true` when an event matches specific criteria:

**Primary Routing (Exclusive)**:
1. Security Critical events → Security Handler
2. Performance Critical OR Warning → Performance Handler
3. Everything else → General Handler

**Secondary Routing (Multi-cast)**:
4. ALL Critical events → Audit Log (regardless of category)

### Predicate Structure
```csharp
// Example predicate structure
Predicate<Event> securityCriticalPredicate = evt => 
    evt.Category == "Security" && evt.Severity == "Critical";
```

### Key Points
- Use `&&` for combining conditions (both must be true)
- Use `||` for alternative conditions (either can be true)
- The audit predicate should ONLY check severity, not category
- Order matters: more specific predicates should be linked first

---

## TODO N2 Hints – Pipeline Assembly

### Pipeline Architecture
```
BroadcastBlock (source)
    ├─[predicate1]→ Security Handler (ActionBlock)
    ├─[predicate2]→ Performance Handler (ActionBlock)
    ├─[predicate3]→ General Handler (ActionBlock)
    └─[predicate4]→ Audit Log (ActionBlock)
```

### LinkTo Syntax
```csharp
var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

source.LinkTo(
    targetBlock,
    linkOptions,
    predicateFunction
);
```

### Critical Considerations
1. **BroadcastBlock**: Use this as your source because it can send the same message to multiple targets
2. **Link Order**: Link specific routes first (security, performance), then general as fallback
3. **PropagateCompletion**: Set to `true` on ALL links so completion flows through
4. **NullTarget**: After all predicated links, link to `DataflowBlock.NullTarget<Event>()` to consume unmatched items

### Common Mistakes
- Forgetting to link general handler (unmatched items get dropped)
- Not setting PropagateCompletion
- Using BufferBlock instead of BroadcastBlock (prevents multi-cast)
- Overlapping primary predicates (causes duplicate processing)

---

## TODO N3 Hints – Completion Verification

### Completion Pattern
```csharp
var completionTask = Task.WhenAll(
    block1.Completion,
    block2.Completion,
    block3.Completion
);

var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
var completed = await Task.WhenAny(completionTask, timeoutTask);

return completed == completionTask;
```

### What to Verify
1. All handler blocks have completed
2. No exceptions occurred (check `Completion.IsFaulted`)
3. Timeout is respected (return false if exceeded)

### Tips
- Use `Task.WhenAll` to combine multiple completion tasks
- Use `Task.WhenAny` with a timeout task
- Check `Task.IsCompletedSuccessfully` for success validation

---

## Common Dataflow Patterns

### Exclusive Routing
Only ONE target receives each message based on first matching predicate:
```csharp
source.LinkTo(highPriority, options, evt => evt.Priority > 5);
source.LinkTo(lowPriority, options, evt => evt.Priority <= 5);
```

### Multi-cast (Audit Tap)
Same message goes to multiple targets (requires BroadcastBlock):
```csharp
broadcast.LinkTo(primaryHandler, options, evt => true);
broadcast.LinkTo(auditLog, options, evt => evt.IsCritical);
```

### Completion Propagation
Ensure downstream blocks complete when upstream completes:
```csharp
var options = new DataflowLinkOptions { PropagateCompletion = true };
```

---

## Testing Strategy

1. **Post Events**: Send test events to the source block
2. **Complete Source**: Call `source.Complete()` to signal no more data
3. **Await Completion**: Wait for all handlers to complete
4. **Verify Routing**: Check each handler received correct event count
5. **Verify Audit**: Confirm audit log captured all critical events

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
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
        private Predicate<Event> SecurityCriticalPredicate => 
            evt => evt.Category == "Security" && evt.Severity == "Critical";

        private Predicate<Event> PerformancePredicate => 
            evt => evt.Category == "Performance" && 
                   (evt.Severity == "Critical" || evt.Severity == "Warning");

        private Predicate<Event> GeneralPredicate => 
            evt => true;

        private Predicate<Event> AuditPredicate => 
            evt => evt.Severity == "Critical";

        private BufferBlock<Event>? _source;
        private ActionBlock<Event>? _securityHandler;
        private ActionBlock<Event>? _performanceHandler;
        private ActionBlock<Event>? _generalHandler;
        private ActionBlock<Event>? _auditLog;

        // TODO[N2]: Build Complete Pipeline with Audit Tap
        public void BuildPipeline()
        {
            // Create source BufferBlock
            _source = new BufferBlock<Event>();

            // Create handler blocks
            _securityHandler = new ActionBlock<Event>(evt =>
            {
                Console.WriteLine($"[SecurityHandler] {evt.Category}.{evt.Severity}: {evt.Message}");
                _securityCount++;
            });

            _performanceHandler = new ActionBlock<Event>(evt =>
            {
                Console.WriteLine($"[PerformanceHandler] {evt.Category}.{evt.Severity}: {evt.Message}");
                _performanceCount++;
            });

            _generalHandler = new ActionBlock<Event>(evt =>
            {
                Console.WriteLine($"[GeneralHandler] {evt.Category}.{evt.Severity}: {evt.Message}");
                _generalCount++;
            });

            _auditLog = new ActionBlock<Event>(evt =>
            {
                Console.WriteLine($"[AuditLog] CRITICAL: {evt.Message}");
                _auditCount++;
            });

            // Configure link options with completion propagation
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // Create a broadcast block for multi-casting critical events
            var criticalBroadcast = new BroadcastBlock<Event>(evt => evt);
            
            // First link: route critical events to broadcaster
            _source.LinkTo(criticalBroadcast, linkOptions, AuditPredicate);
            
            // From broadcaster: route to specific handlers based on category AND send to audit log
            criticalBroadcast.LinkTo(_securityHandler, linkOptions, evt => evt.Category == "Security");
            criticalBroadcast.LinkTo(_performanceHandler, linkOptions, evt => evt.Category == "Performance");
            criticalBroadcast.LinkTo(_generalHandler, linkOptions, evt => evt.Category == "Application");
            criticalBroadcast.LinkTo(_auditLog, linkOptions, evt => true);
            
            // Route non-critical events directly from source based on category
            _source.LinkTo(_performanceHandler, linkOptions, evt => evt.Category == "Performance");
            _source.LinkTo(_generalHandler, linkOptions, evt => true); // Catch all remaining
            
            // Link remaining to NullTarget
            _source.LinkTo(DataflowBlock.NullTarget<Event>());
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
            if (_securityHandler == null || _performanceHandler == null || 
                _generalHandler == null || _auditLog == null)
            {
                return false;
            }

            var completionTask = Task.WhenAll(
                _securityHandler.Completion,
                _performanceHandler.Completion,
                _generalHandler.Completion,
                _auditLog.Completion
            );

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(completionTask, timeoutTask);

            return completedTask == completionTask && completionTask.IsCompletedSuccessfully;
        }
    }
}
```

</details>

