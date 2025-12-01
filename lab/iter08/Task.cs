using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter08
{
    public record Order(string CustomerName, string Email, decimal Amount, bool IsExpress);

    // TODO N1 – Implement Result Type and Validation
    // 
    // Create a Result<T> record type that represents success or failure WITHOUT exceptions.
    // Required properties:
    //   - IsSuccess: bool indicating success
    //   - Value: T? containing the value on success (null on failure)
    //   - ErrorMessage: string? containing error message on failure (null on success)
    // 
    // Required static factory methods:
    //   - Success(T value): Returns a successful Result<T>
    //   - Failure(string error): Returns a failed Result<T>
    // 
    // [YOUR CODE GOES HERE]
    public record Result<T>
    {
        public bool IsSuccess { get; init; }
        public T? Value { get; init; }
        public string? ErrorMessage { get; init; }

        public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
        public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, ErrorMessage = error };
    }

    // TODO N3 – Implement Statistics Tracking
    // 
    // Create a ProcessorStats class to track processing metrics in a thread-safe manner.
    // Required properties (read-only):
    //   - TotalProcessed: Total orders received
    //   - SuccessCount: Successfully validated orders
    //   - FailureCount: Failed validation orders
    //   - ExpressCount: Express orders (successful only)
    //   - StandardCount: Standard orders (successful only)
    // 
    // Required methods (use Interlocked.Increment for thread safety):
    //   - IncrementTotal(): Increment total count
    //   - IncrementSuccess(): Increment success count
    //   - IncrementFailure(): Increment failure count
    //   - IncrementExpress(): Increment express count
    //   - IncrementStandard(): Increment standard count
    // 
    // [YOUR CODE GOES HERE]
public class ProcessorStats
    {
        private int _totalProcessed;
        private int _successCount;
        private int _failureCount;
        private int _expressCount;
        private int _standardCount;

        public int TotalProcessed => Volatile.Read(ref _totalProcessed);
        public int SuccessCount => Volatile.Read(ref _successCount);
        public int FailureCount => Volatile.Read(ref _failureCount);
        public int ExpressCount => Volatile.Read(ref _expressCount);
        public int StandardCount => Volatile.Read(ref _standardCount);

        public void IncrementTotal() => Interlocked.Increment(ref _totalProcessed);
        public void IncrementSuccess() => Interlocked.Increment(ref _successCount);
        public void IncrementFailure() => Interlocked.Increment(ref _failureCount);
        public void IncrementExpress() => Interlocked.Increment(ref _expressCount);
        public void IncrementStandard() => Interlocked.Increment(ref _standardCount);
    }

    public class OrderProcessor
    {
        // TODO N1 – Implement Result Type and Validation
        // 
        // Implement ValidateOrder to check:
        //   1. order.Amount > 0 (return Failure("Invalid amount") if not)
        //   2. order.Email.Contains("@") (return Failure("Invalid email") if not)
        //   3. Return Success(order) if all validations pass
        // 
        // CRITICAL: Never throw exceptions - always return Result<Order>
        // 
        // [YOUR CODE GOES HERE]

        public static Result<Order> ValidateOrder(Order order)
        {
            if (order.Amount <= 0)
            {
                return Result<Order>.Failure("Invalid amount");
            }
            else if (!order.Email.Contains("@"))
            {
                return Result<Order>.Failure("Invalid email");
            }
            return Result<Order>.Success(order);
        }

        // TODO N2 – Build Encapsulated Pipeline
        // 
        // Create a resilient order processing pipeline using DataflowBlock.Encapsulate.
        // 
        // Pipeline architecture (4 stages):
        //   1. Entry: BufferBlock<Order>
        //   2. Validator: TransformBlock<Order, Result<Order>>
        //      - Call stats.IncrementTotal()
        //      - Call ValidateOrder(order)
        //      - If success: call stats.IncrementSuccess()
        //      - If failure: call stats.IncrementFailure()
        //      - Return result
        //   3. Pricer: TransformBlock<Result<Order>, Result<Order>>
        //      - Pass through failures unchanged
        //      - For successes: if Amount > 1000, apply 10% discount
        //   4. Router: TransformBlock<Result<Order>, Result<Order>>
        //      - Pass through failures unchanged
        //      - For successes: track express/standard stats
        //   5. Exit: BufferBlock<Result<Order>>
        // 
        // CRITICAL: Use DataflowLinkOptions { PropagateCompletion = true } on ALL links
        // 
        // Return: DataflowBlock.Encapsulate(entry, exit)
        // 
        // [YOUR CODE GOES HERE]
        public static IPropagatorBlock<Order, Result<Order>> CreateProcessor(ProcessorStats stats)
        {
            var input = new BufferBlock<Order>(new DataflowBlockOptions
            {
                BoundedCapacity = 1024,
                EnsureOrdered = false,
            });

            var validator = new TransformBlock<Order, Result<Order>>((o) =>
            {
                stats.IncrementTotal();
                var result = ValidateOrder(o);
                if (result.IsSuccess)
                {
                    stats.IncrementSuccess();
                }
                else
                {
                    stats.IncrementFailure();
                }
                return result;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1024,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                EnsureOrdered = false
            });

            var pricer = new TransformBlock<Result<Order>, Result<Order>>(o =>
            {
                if (o.IsSuccess && o.Value.Amount > 1000)
                {
                    var discounted = new Order(o.Value.CustomerName, o.Value.Email, o.Value.Amount * (decimal) 0.9, o.Value.IsExpress);
                    return Result<Order>.Success(discounted);
                }
                return o;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1024,
                EnsureOrdered = false,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });

            var router = new TransformBlock<Result<Order>, Result<Order>>(o =>
            {
                if (o.IsSuccess)
                {
                    if (o.Value.IsExpress)
                        stats.IncrementExpress();
                    else
                        stats.IncrementStandard();
                }
                return o;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1024,
                EnsureOrdered = false,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });

            var output = new BufferBlock<Result<Order>>(new DataflowBlockOptions
            {
                BoundedCapacity = 1024,
                EnsureOrdered = false,
            });

            var linkOpts = new DataflowLinkOptions
            {
                PropagateCompletion = true,
            };

            input.LinkTo(validator, linkOpts);
            validator.LinkTo(pricer, linkOpts);
            pricer.LinkTo(router, linkOpts);
            router.LinkTo(output, linkOpts);

            return DataflowBlock.Encapsulate(input, output);
        }
    }
}
