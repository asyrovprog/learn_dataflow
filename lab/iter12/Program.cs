using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab.Iter12;

Console.WriteLine("=== Lab Iter12: Search Autocomplete with Rx.NET ===\n");

var search = new SearchAutocomplete();

try
{
    search.StartSearchPipeline();
}
catch (NotImplementedException ex)
{
    var todoId = ex.Message;
    var sectionMap = new Dictionary<string, string>
    {
        ["TODO[N1]"] = "TODO N1 – Create Debounced Search Observable",
        ["TODO[N2]"] = "TODO N2 – Transform to API Calls with SelectMany",
        ["TODO[N3]"] = "TODO N3 – Subscribe and Handle Results"
    };
    Console.WriteLine($"❌ FAIL: {todoId} not satisfied – see README section '{sectionMap[todoId]}'");
    return;
}

// Test 1: Rapid keystrokes should be debounced
Console.WriteLine("Test 1: Debouncing rapid keystrokes...");
search.SimulateKeystroke("r");
await Task.Delay(100);
search.SimulateKeystroke("re");
await Task.Delay(100);
search.SimulateKeystroke("rea");
await Task.Delay(100);
search.SimulateKeystroke("reac");
await Task.Delay(400); // Wait for debounce + API call

var apiLog1 = search.GetApiCallLog();
if (apiLog1.Count == 3 && apiLog1[0] == "Result1:reac")
{
    Console.WriteLine("✅ PASS: Only final keystroke triggered API call");
}
else
{
    Console.WriteLine($"❌ FAIL: Expected 3 results for 'reac', got {apiLog1.Count} results");
    return;
}

// Test 2: Empty/whitespace should be filtered
Console.WriteLine("\nTest 2: Filtering empty strings...");
var search2 = new SearchAutocomplete();
search2.StartSearchPipeline();
search2.SimulateKeystroke("test");
await Task.Delay(400); // Wait for debounce + API
search2.SimulateKeystroke("   ");
await Task.Delay(100);
search2.SimulateKeystroke("");
await Task.Delay(400);

var apiLog2 = search2.GetApiCallLog();
if (apiLog2.Count == 3 && apiLog2[0].Contains("test"))
{
    Console.WriteLine("✅ PASS: Empty/whitespace strings filtered out");
}
else
{
    Console.WriteLine($"❌ FAIL: Expected only 'test' results, got {apiLog2.Count} results");
    return;
}

// Test 3: Multiple debounced searches
Console.WriteLine("\nTest 3: Multiple debounced searches...");
var search3 = new SearchAutocomplete();
search3.StartSearchPipeline();
search3.SimulateKeystroke("java");
await Task.Delay(400);
search3.SimulateKeystroke("python");
await Task.Delay(400);

var apiLog3 = search3.GetApiCallLog();
if (apiLog3.Count == 6 && 
    apiLog3[0] == "Result1:java" && 
    apiLog3[3] == "Result1:python")
{
    Console.WriteLine("✅ PASS: Multiple searches processed correctly");
}
else
{
    Console.WriteLine($"❌ FAIL: Expected 6 results (3 per search), got {apiLog3.Count}");
    return;
}

// Test 4: Disposal cleanup
Console.WriteLine("\nTest 4: Subscription disposal...");
var search4 = new SearchAutocomplete();
search4.StartSearchPipeline();
search4.SimulateKeystroke("dispose");
await Task.Delay(100);
search4.Dispose();
await Task.Delay(400);

var apiLog4 = search4.GetApiCallLog();
if (apiLog4.Count == 0)
{
    Console.WriteLine("✅ PASS: Disposed before debounce completed, no API calls");
}
else
{
    Console.WriteLine($"❌ FAIL: Expected 0 results after disposal, got {apiLog4.Count}");
    return;
}

Console.WriteLine("\n✅ ALL TESTS PASSED!");
Console.WriteLine("\nCongratulations! You've successfully implemented:");
Console.WriteLine("- Throttle operator for debouncing user input");
Console.WriteLine("- SelectMany for async transformation");
Console.WriteLine("- Proper subscription management and disposal");
