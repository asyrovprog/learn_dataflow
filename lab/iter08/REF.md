# Lab 08 Reference Guide

## TODO N1 Hints – Result Type and Validation

### Understanding Result<T> Pattern
The Result<T> pattern represents an operation that can succeed or fail WITHOUT throwing exceptions:

```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }          // Only populated on success
    public string? ErrorMessage { get; init; }  // Only populated on failure
}
```

### Creating Results
Provide factory methods for convenience:
```csharp
public static Result<T> Success(T value) =>
    new Result<T> { IsSuccess = true, Value = value };

public static Result<T> Failure(string error) =>
    new Result<T> { IsSuccess = false, ErrorMessage = error };
```

### Validation Logic
```csharp
public static Result<Order> ValidateOrder(Order order)
{
    // Check amount
    if (order.Amount <= 0)
        return Result<Order>.Failure("Invalid amount");
    
    // Check email
    if (!order.Email.Contains("@"))
        return Result<Order>.Failure("Invalid email");
    
    // All validations passed
    return Result<Order>.Success(order);
}
```

### Key Points
- **Never throw** - Return Failure() instead
- **Check all conditions** - Validate amount and email
- **Clear error messages** - Help debugging
- **Type-safe** - Result<T> carries the value type

---

## TODO N2 Hints – Encapsulated Pipeline

### Pipeline Architecture
```
BufferBlock<Order> (entry)
    ↓
TransformBlock (validate → Result<Order>)
    ↓
TransformBlock (apply pricing → Result<Order>)
    ↓
TransformBlock (route & track stats → Result<Order>)
    ↓
BufferBlock<Result<Order>> (exit)
```

### Stage 1: Validation Block
```csharp
var validator = new TransformBlock<Order, Result<Order>>(order =>
{
    stats.IncrementTotal();
    var result = ValidateOrder(order);
    
    if (result.IsSuccess)
        stats.IncrementSuccess();
    else
        stats.IncrementFailure();
    
    return result;
});
```

### Stage 2: Pricing Block (Pass Through Failures)
```csharp
var pricer = new TransformBlock<Result<Order>, Result<Order>>(result =>
{
    if (!result.IsSuccess)
        return result; // ✅ Pass through failures unchanged
    
    var order = result.Value!;
    if (order.Amount > 1000)
    {
        var discounted = order with { Amount = order.Amount * 0.9m };
        return Result<Order>.Success(discounted);
    }
    
    return result;
});
```

### Stage 3: Routing & Stats
```csharp
var router = new TransformBlock<Result<Order>, Result<Order>>(result =>
{
    if (result.IsSuccess)
    {
        if (result.Value!.IsExpress)
            stats.IncrementExpress();
        else
            stats.IncrementStandard();
    }
    return result;
});
```

### Linking with PropagateCompletion
```csharp
var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

input.LinkTo(validator, linkOptions);
validator.LinkTo(pricer, linkOptions);
pricer.LinkTo(router, linkOptions);
router.LinkTo(output, linkOptions);
```

### Encapsulation
```csharp
return DataflowBlock.Encapsulate(input, output);
```

### Critical Points
- ✅ **PropagateCompletion = true** on ALL links
- ✅ **Pass failures through** each stage
- ✅ **Entry point** - BufferBlock<Order>
- ✅ **Exit point** - BufferBlock<Result<Order>>
- ✅ **Encapsulate** wraps entry and exit

---

## TODO N3 Hints – Statistics Tracking

### ProcessorStats Class
```csharp
public class ProcessorStats
{
    private int _totalProcessed = 0;
    private int _successCount = 0;
    private int _failureCount = 0;
    private int _expressCount = 0;
    private int _standardCount = 0;
    
    public int TotalProcessed => _totalProcessed;
    public int SuccessCount => _successCount;
    public int FailureCount => _failureCount;
    public int ExpressCount => _expressCount;
    public int StandardCount => _standardCount;
    
    public void IncrementTotal() => Interlocked.Increment(ref _totalProcessed);
    public void IncrementSuccess() => Interlocked.Increment(ref _successCount);
    public void IncrementFailure() => Interlocked.Increment(ref _failureCount);
    public void IncrementExpress() => Interlocked.Increment(ref _expressCount);
    public void IncrementStandard() => Interlocked.Increment(ref _standardCount);
}
```

### Where to Update Counters
- **Total** - In validator (every order)
- **Success/Failure** - In validator (after validation)
- **Express/Standard** - In router (only for successful orders)

### Thread Safety
Use `Interlocked.Increment()` because multiple blocks may execute in parallel:
```csharp
Interlocked.Increment(ref _totalProcessed);
```

---

## Common Patterns

### Passing Failures Through Stages
```csharp
var transform = new TransformBlock<Result<T>, Result<T>>(result =>
{
    if (!result.IsSuccess)
        return result; // Early return for failures
    
    // Process successful results only
    var value = result.Value!;
    // ... do work ...
    return Result<T>.Success(processedValue);
});
```

### Encapsulation Pattern
```csharp
// 1. Create entry and exit
var entry = new BufferBlock<TInput>();
var exit = new BufferBlock<TOutput>();

// 2. Create and link internal stages
// ... stage creation and linking ...

// 3. Encapsulate
return DataflowBlock.Encapsulate(entry, exit);
```

---

<details><summary>Reference Solution (open after completion)</summary>

```csharp
using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter08
{
    public record Order(string CustomerName, string Email, decimal Amount, bool IsExpress);

    // TODO[N1]: Implement Result Type and Validation
    public record Result<T>
    {
        public bool IsSuccess { get; init; }
        public T? Value { get; init; }
        public string? ErrorMessage { get; init; }

        public static Result<T> Success(T value) =>
            new Result<T> { IsSuccess = true, Value = value };

        public static Result<T> Failure(string error) =>
            new Result<T> { IsSuccess = false, ErrorMessage = error };
    }

    // TODO[N3]: Implement Statistics Tracking
    public class ProcessorStats
    {
        private int _totalProcessed = 0;
        private int _successCount = 0;
        private int _failureCount = 0;
        private int _expressCount = 0;
        private int _standardCount = 0;

        public int TotalProcessed => _totalProcessed;
        public int SuccessCount => _successCount;
        public int FailureCount => _failureCount;
        public int ExpressCount => _expressCount;
        public int StandardCount => _standardCount;

        public void IncrementTotal() => Interlocked.Increment(ref _totalProcessed);
        public void IncrementSuccess() => Interlocked.Increment(ref _successCount);
        public void IncrementFailure() => Interlocked.Increment(ref _failureCount);
        public void IncrementExpress() => Interlocked.Increment(ref _expressCount);
        public void IncrementStandard() => Interlocked.Increment(ref _standardCount);
    }

    public class OrderProcessor
    {
        // TODO[N1]: Implement Result Type and Validation
        public static Result<Order> ValidateOrder(Order order)
        {
            if (order.Amount <= 0)
                return Result<Order>.Failure("Invalid amount");

            if (!order.Email.Contains("@"))
                return Result<Order>.Failure("Invalid email");

            return Result<Order>.Success(order);
        }

        // TODO[N2]: Build Encapsulated Pipeline
        public static IPropagatorBlock<Order, Result<Order>> CreateProcessor(ProcessorStats stats)
        {
            // Entry point
            var input = new BufferBlock<Order>();

            // Stage 1: Validation
            var validator = new TransformBlock<Order, Result<Order>>(order =>
            {
                stats.IncrementTotal();
                var result = ValidateOrder(order);
                
                if (result.IsSuccess)
                    stats.IncrementSuccess();
                else
                    stats.IncrementFailure();
                
                return result;
            });

            // Stage 2: Pricing - apply discounts
            var pricer = new TransformBlock<Result<Order>, Result<Order>>(result =>
            {
                if (!result.IsSuccess)
                    return result; // Pass through failures

                var order = result.Value!;
                if (order.Amount > 1000)
                {
                    // Apply 10% discount for orders over $1000
                    var discountedOrder = order with { Amount = order.Amount * 0.9m };
                    return Result<Order>.Success(discountedOrder);
                }

                return result;
            });

            // Stage 3: Route by category and track stats
            var router = new TransformBlock<Result<Order>, Result<Order>>(result =>
            {
                if (result.IsSuccess)
                {
                    if (result.Value!.IsExpress)
                        stats.IncrementExpress();
                    else
                        stats.IncrementStandard();
                }
                return result;
            });

            // Stage 4: Output
            var output = new BufferBlock<Result<Order>>();

            // Link pipeline
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            input.LinkTo(validator, linkOptions);
            validator.LinkTo(pricer, linkOptions);
            pricer.LinkTo(router, linkOptions);
            router.LinkTo(output, linkOptions);

            // Encapsulate
            return DataflowBlock.Encapsulate(input, output);
        }
    }
}
```

</details>
