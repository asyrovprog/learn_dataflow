# Lab 03: Resilient Data Import Pipeline

## Overview
Build a dataflow pipeline that imports CSV data representing user registrations. Some lines may be malformed, have missing fields, or contain invalid data types. Your pipeline should handle errors gracefully, log issues, and continue processing valid records.

## Scenario
You're processing user registration data in CSV format:
```
UserID,Name,Age,Email
1,Alice,25,alice@example.com
2,Bob,30,bob@example.com
3,Charlie,invalid,charlie@example.com
4,,28,dave@example.com
5,Eve,22,
```

Lines may have issues:
- Invalid data types (e.g., "invalid" for Age)
- Missing required fields (empty Name or Email)
- Wrong number of fields

Your pipeline should:
1. Parse CSV lines into structured records
2. Route valid records to a success path
3. Route invalid records to an error path with descriptive error messages
4. Report final statistics (success count, error count, error details)

## TODO T1 – Parse and Validate CSV Lines

**Objective:** Implement `ParseCsvLine()` to parse a CSV line and return a `ParseResult`.

**Requirements:**
- Split the line by commas to extract fields
- Check that there are exactly 4 fields (UserID, Name, Age, Email)
- Validate that UserID can be parsed as an integer
- Validate that Name is not empty
- Validate that Age can be parsed as an integer and is between 1-120
- Validate that Email is not empty and contains '@'
- If all validations pass, return `ParseResult.Success()` with the user data
- If any validation fails, return `ParseResult.Failure()` with a descriptive error message

**Hints:**
- Use `string.Split(',')` to parse fields
- Use `int.TryParse()` to safely parse integers
- Build descriptive error messages like "Invalid Age: must be 1-120" or "Missing Name field"

## TODO T2 – Build Error-Handling Pipeline

**Objective:** Implement `CreateImportPipeline()` to build a dataflow pipeline that processes CSV lines with error handling.

**Requirements:**
- Create a `TransformBlock<string, ParseResult>` that parses each line using `ParseCsvLine()`
- Use `LinkTo()` with predicates to route successful results to one path and failures to another
- For successful results: collect the user data into a concurrent collection
- For failed results: collect the error messages into a concurrent collection
- Return a tuple containing (parseBlock, successCount, errorMessages)

**Hints:**
- Use `LinkTo()` twice on the same source block with different predicates: `result => result.IsSuccess` and `result => !result.IsSuccess`
- Use `ActionBlock` to handle each path (collecting data or errors)
- Remember to propagate completion between blocks
- Use thread-safe collections like `ConcurrentBag<T>`

## TODO T3 – Implement Statistics Reporting

**Objective:** Implement `ReportStatistics()` to display pipeline processing results.

**Requirements:**
- Print total records processed
- Print success count and error count
- For each error, print the line number and error message
- Calculate and display success rate as a percentage

**Hints:**
- Format output clearly for easy reading
- Use `$"..."` string interpolation
- Round percentages to 1 decimal place
