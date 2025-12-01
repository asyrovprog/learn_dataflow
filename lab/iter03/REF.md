# Lab 03 Reference - Resilient Data Import Pipeline

## TODO T1 Hints – Parse and Validate CSV Lines

**Key Concepts:**
- Use `string.Split(',')` to parse CSV fields
- Use `int.TryParse()` for safe integer parsing
- Check for null/empty strings with `string.IsNullOrWhiteSpace()`
- Return descriptive error messages for each validation failure

**Approach:**
1. Split the line into fields (should be exactly 4)
2. Trim whitespace from each field
3. Validate each field in order:
   - UserID must parse as int
   - Name must not be empty
   - Age must parse as int and be between 1-120
   - Email must not be empty and contain '@'
4. Return `ParseResult.Success()` if all validations pass
5. Return `ParseResult.Failure()` with descriptive error message otherwise

**Common Pitfalls:**
- Forgetting to trim whitespace before validation
- Not providing specific error messages (e.g., "Invalid Age: must be 1-120, got 150")
- Not checking field count before accessing array elements

## TODO T2 Hints – Build Error-Handling Pipeline

**Key Concepts:**
- Use `TransformBlock` to parse each CSV line
- Use `LinkTo()` with predicates to route success vs failure
- Need two separate paths: one for valid data, one for errors
- Use `ConcurrentBag` for thread-safe collection

**Approach:**
1. Create concurrent collections for success data and error messages
2. Create a `TransformBlock<string, ParseResult>` that calls `ParseCsvLine()`
3. Create an `ActionBlock<ParseResult>` for successful results (collect UserData)
4. Create an `ActionBlock<ParseResult>` for failures (collect error messages)
5. Link the transform block to success block with predicate `result => result.IsSuccess`
6. Link the transform block to error block with predicate `result => !result.IsSuccess`
7. Set `PropagateCompletion = true` on both links
8. Return the parse block as the input, along with the collections

**Common Pitfalls:**
- Forgetting to set `PropagateCompletion = true`
- Not using predicates on `LinkTo()` calls
- Trying to access `Data` on failure results or `ErrorMessage` on success results

## TODO T3 Hints – Implement Statistics Reporting

**Key Concepts:**
- Print summary statistics about processing results
- Display errors with line numbers
- Calculate success rate percentage

**Approach:**
1. Print header "=== Import Statistics ==="
2. Print total records, successful count, failed count
3. If there are errors, iterate through error messages with line numbers
4. Calculate success rate: `(successCount * 100.0 / totalRecords)`
5. Format percentage to 1 decimal place using `:F1`

**Common Pitfalls:**
- Integer division (use 100.0, not 100)
- Not handling divide-by-zero when totalRecords is 0
- Forgetting to format percentage properly

---

<details>
<summary>Reference Solution (open after completion)</summary>

```csharp
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
        var fields = line.Split(',');
        
        if (fields.Length != 4)
        {
            return ParseResult.Failure($"Invalid field count: expected 4, got {fields.Length}");
        }

        var userIdStr = fields[0].Trim();
        var name = fields[1].Trim();
        var ageStr = fields[2].Trim();
        var email = fields[3].Trim();

        if (!int.TryParse(userIdStr, out int userId))
        {
            return ParseResult.Failure($"Invalid UserID: '{userIdStr}' is not a valid integer");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return ParseResult.Failure("Missing Name field");
        }

        if (!int.TryParse(ageStr, out int age))
        {
            return ParseResult.Failure($"Invalid Age: '{ageStr}' is not a valid integer");
        }

        if (age < 1 || age > 120)
        {
            return ParseResult.Failure($"Invalid Age: must be 1-120, got {age}");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ParseResult.Failure("Missing Email field");
        }

        if (!email.Contains('@'))
        {
            return ParseResult.Failure($"Invalid Email: '{email}' must contain '@'");
        }

        return ParseResult.Success(new UserData(userId, name, age, email));
    }

    // TODO[T2]: Build Error-Handling Pipeline
    public static (ITargetBlock<string> inputBlock, ConcurrentBag<UserData> successData, ConcurrentBag<string> errorMessages) CreateImportPipeline()
    {
        var successData = new ConcurrentBag<UserData>();
        var errorMessages = new ConcurrentBag<string>();

        var parseBlock = new TransformBlock<string, ParseResult>(
            line => ParseCsvLine(line));

        var successBlock = new ActionBlock<ParseResult>(
            result => successData.Add(result.Data!));

        var errorBlock = new ActionBlock<ParseResult>(
            result => errorMessages.Add(result.ErrorMessage!));

        parseBlock.LinkTo(successBlock, new DataflowLinkOptions { PropagateCompletion = true }, result => result.IsSuccess);
        parseBlock.LinkTo(errorBlock, new DataflowLinkOptions { PropagateCompletion = true }, result => !result.IsSuccess);

        return (parseBlock, successData, errorMessages);
    }

    // TODO[T3]: Implement Statistics Reporting
    public static void ReportStatistics(int totalRecords, int successCount, int errorCount, List<string> errorMessages)
    {
        Console.WriteLine($"\n=== Import Statistics ===");
        Console.WriteLine($"Total Records: {totalRecords}");
        Console.WriteLine($"Successful: {successCount}");
        Console.WriteLine($"Failed: {errorCount}");

        if (errorCount > 0)
        {
            Console.WriteLine($"\nErrors:");
            int lineNum = 1;
            foreach (var error in errorMessages)
            {
                Console.WriteLine($"  Line {lineNum}: {error}");
                lineNum++;
            }
        }

        double successRate = totalRecords > 0 ? (successCount * 100.0 / totalRecords) : 0;
        Console.WriteLine($"\nSuccess Rate: {successRate:F1}%");
    }
}
```

</details>
