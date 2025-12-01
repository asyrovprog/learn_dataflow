using Lab.Iter05;

Console.WriteLine("=== Lab Iter05: Multi-Source Weather Dashboard ===\n");

// Test data
var cities = new List<string> { "Seattle", "Portland", "San Francisco" };

// Capture output for verification
var originalOut = Console.Out;
var output = new StringWriter();
Console.SetOut(output);

try
{
    await WeatherDashboard.RunDashboardAsync(cities);
}
catch (NotImplementedException ex)
{
    Console.SetOut(originalOut);
    var todoId = ex.Message;
    var sectionMap = new Dictionary<string, string>
    {
        ["TODO[N1]"] = "TODO N1 – Create Weather Data Fetchers",
        ["TODO[N2]"] = "TODO N2 – Build JoinBlock Pipeline",
        ["TODO[N3]"] = "TODO N3 – Process Cities and Verify Synchronization"
    };

    Console.WriteLine($"❌ FAIL: {todoId} not satisfied – see README section '{sectionMap[todoId]}'");
    return;
}

Console.SetOut(originalOut);
var result = output.ToString();

// Verify test conditions
var tests = new List<(string name, Func<bool> check)>
{
    ("All three cities appear in output", () =>
        result.Contains("Seattle") && result.Contains("Portland") && result.Contains("San Francisco")),

    ("Weather data is present", () =>
        result.Contains("52°F, Rainy") && result.Contains("58°F, Cloudy") && result.Contains("65°F, Sunny")),

    ("Air Quality data is present", () =>
        result.Contains("Good (45 AQI)") && result.Contains("Moderate (78 AQI)") && result.Contains("Good (55 AQI)")),

    ("Traffic data is present", () =>
        result.Contains("Moderate") && result.Contains("Light") && result.Contains("Heavy")),

    ("Data is synchronized (each city appears once in complete report)", () =>
    {
        var reports = result.Split("=== City Status Report ===", StringSplitOptions.RemoveEmptyEntries);
        return reports.Length == 4; // 3 reports + 1 empty initial split
    }),

    ("Seattle report has all three data types together", () =>
    {
        var seattleIndex = result.IndexOf("City: Seattle");
        if (seattleIndex == -1) return false;
        var nextReport = result.IndexOf("=== City Status Report ===", seattleIndex + 1);
        var seattleSection = nextReport == -1 ? result[seattleIndex..] : result[seattleIndex..nextReport];
        return seattleSection.Contains("52°F") && seattleSection.Contains("45 AQI") && seattleSection.Contains("Moderate");
    })
};

Console.WriteLine("Running tests...\n");
var passed = 0;
var total = tests.Count;

foreach (var (name, check) in tests)
{
    try
    {
        if (check())
        {
            Console.WriteLine($"✅ PASS: {name}");
            passed++;
        }
        else
        {
            Console.WriteLine($"❌ FAIL: {name}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAIL: {name} (Exception: {ex.Message})");
    }
}

Console.WriteLine($"\n{passed}/{total} tests passed");

if (passed == total)
{
    Console.WriteLine("\n🎉 All tests passed! Lab completed successfully.");
    Console.WriteLine("\nActual output:");
    Console.WriteLine(result);
}
else
{
    Console.WriteLine("\n❌ Some tests failed. Review the TODO sections in README.md");
}