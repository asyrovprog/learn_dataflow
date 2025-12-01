using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter03;

public record UserData(int UserId, string Name, int Age, string Email);

public class ParseResult
{
    public bool IsSuccess { get; }
    public UserData? Data { get; }
    public string? ErrorMessage { get; }

    private ParseResult(bool isSuccess, UserData? data, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static ParseResult Success(UserData data) => new(true, data, null);
    public static ParseResult Failure(string errorMessage) => new(false, null, errorMessage);
}

public static class DataImporter
{
    // TODO[T1]: Parse and Validate CSV Lines
    public static ParseResult ParseCsvLine(string line)
    {
        /*
            Requirements:

                Split the line by commas to extract fields
                Check that there are exactly 4 fields (UserID, Name, Age, Email)
                Validate that UserID can be parsed as an integer
                Validate that Name is not empty
                Validate that Age can be parsed as an integer and is between 1-120
                Validate that Email is not empty and contains '@'
                If all validations pass, return ParseResult.Success() with the user data
                If any validation fails, return ParseResult.Failure() with a descriptive error message

            Hints:
                Use string.Split(',') to parse fields
                Use int.TryParse() to safely parse integers
                Build descriptive error messages like "Invalid Age: must be 1-120" or "Missing Name field"

            For invalid age with wrong type: Should include the actual invalid value, e.g., "Invalid Age: 'invalid' is not a valid integer"
            For missing fields: Should say exactly "Missing Name field" or "Missing Email field"
            For age out of range: Should include both the range and actual value, e.g., "Invalid Age: must be 1-120, got 150"
            For email validation: Should mention the '@' requirement, e.g., "Invalid Email: 'grace.com' must contain '@'"                
        */
        var parts = line.Split(',');
        
        if (parts.Length != 4) return ParseResult.Failure("Expected 4 parts");
        if (!int.TryParse(parts[0].Trim(), out var userId)) return ParseResult.Failure("Invalid UserId");
        
        var name = parts[1];
        if (string.IsNullOrWhiteSpace(name)) return ParseResult.Failure("Missing Name field");

        if (!int.TryParse(parts[2].Trim(), out var age)) return ParseResult.Failure("Invalid Age: 'invalid' is not a valid integer");
        if (age < 1 || age > 120) return ParseResult.Failure($"Invalid Age: must be 1-120, got {age}");
        
        var email = parts[3];
        if (string.IsNullOrWhiteSpace(email)) return ParseResult.Failure("Missing Email field");
        if (!email.Contains('@')) return ParseResult.Failure($"Invalid Email: '{email}' must contain '@'");

        return ParseResult.Success(new(userId, name, age, email));
    }

    // TODO[T2]: Build Error-Handling Pipeline
    public static (ITargetBlock<string> inputBlock, ConcurrentBag<UserData> successData, ConcurrentBag<string> errorMessages) CreateImportPipeline()
    {
        /*
        Objective: Implement CreateImportPipeline() to build a dataflow pipeline that processes CSV lines with error handling.

        Requirements:
            Create a TransformBlock<string, ParseResult> that parses each line using ParseCsvLine()
            Use LinkTo() with predicates to route successful results to one path and failures to another
            For successful results: collect the user data into a concurrent collection
            For failed results: collect the error messages into a concurrent collection
            Return a tuple containing (parseBlock, successCount, errorMessages)
        Hints:
            Use LinkTo() twice on the same source block with different predicates: result => result.IsSuccess and result => !result.IsSuccess
            Use ActionBlock to handle each path (collecting data or errors)
            Remember to propagate completion between blocks
            Use thread-safe collections like ConcurrentBag<T>
        */

        ConcurrentBag<string> errorMessages = new();
        ConcurrentBag<UserData> successData = new();

        var transform = new TransformBlock<string, ParseResult>((s) => ParseCsvLine(s), new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = false,
            MaxDegreeOfParallelism = 1,
        });

        int lineId = 0;

        var errors = new ActionBlock<ParseResult>((r) =>
        {
            errorMessages.Add($"{lineId}: {r.ErrorMessage}");
            lineId++;
        }, new ExecutionDataflowBlockOptions()
        {
            BoundedCapacity = 1024,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            EnsureOrdered = false
        });

        var users = new ActionBlock<ParseResult>((r) =>
        {
            successData.Add(r.Data!);
            lineId++;
        }, new ExecutionDataflowBlockOptions()
        {
            BoundedCapacity = 1024,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            EnsureOrdered = false
        });

        transform.LinkTo(errors, new DataflowLinkOptions
        {
            PropagateCompletion = true,
        }, (r) => r.Data == null);

        transform.LinkTo(users, new DataflowLinkOptions
        {
            PropagateCompletion = true,
        }, (r) => r.Data != null);


        return (transform, successData, errorMessages);
    }

    // TODO[T3]: Implement Statistics Reporting
    public static void ReportStatistics(int totalRecords, int successCount, int errorCount, List<string> errorMessages)
    {
        /*
        **Objective:** Implement `ReportStatistics()` to display pipeline processing results.

        **Requirements:**
            - Print total records processed
            - Print success count and error count
            - For each error, print the line number and error message
            - Calculate and display success rate as a percentage
        */

        Console.WriteLine($"Total Records Processed: {totalRecords}");
        Console.WriteLine($"Success count: {successCount}");
        Console.WriteLine($"Error count: {errorCount}");
        foreach (var err in errorMessages)
        {
            Console.WriteLine(err);
        }
        Console.WriteLine($"Success Rate: {((double) successCount) / (successCount + errorCount)}");
    }
}
