# Lab 06 - Smart Event Router

## Overview
Build a dataflow pipeline that routes incoming events to different handlers based on **severity** and **category** predicates. This lab demonstrates predicate-based routing, audit taps, and completion propagation in a multi-path architecture.

## Scenario
Your system receives events from multiple sources. Each event has:
- **Category**: "Security", "Performance", "Application"
- **Severity**: "Critical", "Warning", "Info"

Requirements:
1. Route events to specialized handlers based on predicates
2. Tap all Critical events to an audit log (multi-cast)
3. Track completion across all routes
4. Maintain proper backpressure and completion propagation

## TODO N1 – Implement Predicate-Based Routing Logic

**Objective**: Create routing predicates that direct events to appropriate handlers based on severity and category combinations.

**Requirements**:
- Security Critical → Security Handler
- Performance Critical/Warning → Performance Handler  
- All other events → General Handler
- All Critical events should ALSO go to Audit Log (multi-cast)

**Key Concepts**:
- Exclusive routing (one primary path per item)
- Predicate functions in LinkTo
- Multi-cast via multiple links from same source

## TODO N2 – Build Complete Pipeline with Audit Tap

**Objective**: Assemble the full dataflow pipeline connecting source block through routing predicates to handler blocks, including an audit tap for critical events.

**Requirements**:
- Use BroadcastBlock as source to enable multi-cast
- Link to Security, Performance, and General handlers with appropriate predicates
- Add audit tap link WITHOUT consuming the message (predicates must not overlap on primary paths)
- Configure PropagateCompletion on all links
- Ensure proper completion flow to all endpoints

**Key Concepts**:
- BroadcastBlock for multi-cast capability
- LinkTo with predicate parameters
- PropagateCompletion for graceful shutdown
- Non-overlapping predicates for exclusive routing

## TODO N3 – Verify Pipeline Completion (Optional Challenge)

**Objective**: Implement completion verification logic that ensures all handler blocks complete only after the source completes and all messages are processed.

**Requirements**:
- Wait for all handler blocks to complete
- Report completion status for each handler
- Handle timeout scenarios (5 seconds max)
- Return true if all complete successfully

**Key Concepts**:
- Task.WhenAll for coordinated completion
- Completion propagation validation
- Timeout handling with Task.Delay

## Expected Output
```
Processing 8 events...
[SecurityHandler] Security.Critical: Unauthorized access attempt
[AuditLog] CRITICAL: Unauthorized access attempt
[PerformanceHandler] Performance.Warning: High memory usage
[GeneralHandler] Application.Info: User logged in
[PerformanceHandler] Performance.Critical: Service timeout
[AuditLog] CRITICAL: Service timeout
[GeneralHandler] Application.Warning: Cache miss
[SecurityHandler] Security.Warning: Multiple login attempts
[GeneralHandler] Security.Info: Password changed

✓ All events processed
✓ Pipeline completed successfully
✓ Security handler completed: 1 events
✓ Performance handler completed: 2 events
✓ General handler completed: 4 events
✓ Audit log completed: 2 critical events
```

## Hints
- See `REF.md` for detailed guidance on predicate composition and pipeline assembly
- Remember: BroadcastBlock clones messages, enabling true multi-cast
- Predicates should be mutually exclusive for primary routing paths
- Audit tap predicate can overlap (severity check only)
