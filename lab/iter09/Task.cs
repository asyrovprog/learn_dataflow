using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter09;

public record ProcessingStats(int Processed, int Skipped, int Failed, int Total);

public class FileProcessor
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ActionBlock<string> _block;
    
    // TODO[N1]: Add thread-safe counters for tracking progress
    // [YOUR CODE GOES HERE]
    // Add three int fields: _processedCount, _skippedCount, _failedCount
    private int _processedCount;
    private int _skippedCount;
    private int _failedCount;
    
    public FileProcessor()
    {
        // TODO[N3]: Pass CancellationToken to block options and async operations
        // [YOUR CODE GOES HERE]
        // Set CancellationToken = _cts.Token in ExecutionDataflowBlockOptions
        _block = new ActionBlock<string>(
            ProcessFileAsync,
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 3,
                CancellationToken = _cts.Token
            });
    }
    
    private async Task ProcessFileAsync(string filePath)
    {
        // TODO[N1]: Check cancellation and track progress with Interlocked
        // [YOUR CODE GOES HERE]
        // Before processing, check if cancellation was requested using _cts.Token.IsCancellationRequested
        // If canceled, increment _skippedCount using Interlocked.Increment and return early
        // On successful processing, increment _processedCount using Interlocked.Increment
        // On exception (any exception), increment _failedCount using Interlocked.Increment
        
        // TODO[N3]: Pass CancellationToken to async operations
        // [YOUR CODE GOES HERE]

        if (_cts.Token.IsCancellationRequested)
        {
            Interlocked.Increment(ref _skippedCount);
            return;
        }

        // Pass _cts.Token to File.ReadAllTextAsync and Task.Delay
        try
        {
            var content = await File.ReadAllTextAsync(filePath, _cts.Token).ConfigureAwait(false);
            
            // Simulate processing
            await Task.Delay(50, _cts.Token).ConfigureAwait(false);
            
            // Count words
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Interlocked.Increment(ref _processedCount);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref _skippedCount);
        }
        catch
        {
            Interlocked.Increment(ref _failedCount);
        }
    }
    
    public bool Post(string filePath) => _block.Post(filePath);
    
    public void Cancel() => _cts.Cancel();
    
    // TODO[N2]: Implement graceful shutdown with timeout
    // [YOUR CODE GOES HERE]
    // Phase 1: Call _block.Complete() to stop accepting new files
    // Phase 2: Use await _block.Completion.WaitAsync(TimeSpan.FromSeconds(5)) to wait with timeout
    // Phase 3: If TimeoutException is thrown, call _cts.Cancel() and await _block.Completion again
    // Return true if graceful completion succeeded, false if forced cancellation was needed
    public async Task<bool> ShutdownAsync()
    {
        _block.Complete();

        try
        {
            await _block.Completion.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Block was already canceled before we called Complete()
            return false;
        }
        catch (TimeoutException)
        {
            _cts.Cancel();
            
            try
            {
                await _block.Completion.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected during forced cancellation
            }
            
            return false;
        }
    }
    
    // TODO[N1]: Implement GetStats to return current counts
    // [YOUR CODE GOES HERE]
    // Return a ProcessingStats record with current counts
    public ProcessingStats GetStats() => new ProcessingStats(
        this._processedCount, 
        this._skippedCount, 
        this._failedCount, 
        this._processedCount + this._skippedCount + this._failedCount);
}
