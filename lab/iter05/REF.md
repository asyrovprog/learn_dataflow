# Reference Guide: Lab Iter05

## Hints

### TODO N1 – Create Weather Data Fetchers

**Key Concepts:**
- Each method is `async` and returns a `Task<T>` where T is the data type
- Use `await Task.Delay(milliseconds)` to simulate network latency
- Look up data from the provided dictionaries (WeatherData, AirQualityData, TrafficData)
- Return a new record instance with the city name and fetched data

**Structure Pattern:**
```csharp
public static async Task<Weather> FetchWeatherAsync(string city)
{
    await Task.Delay(/* delay time */);
    var data = WeatherData[city];
    return new Weather(/* construct from data */);
}
```

**What to return:**
- `Weather`: city, temperature, condition
- `AirQuality`: city, AQI number, level string
- `Traffic`: city, status string

---

### TODO N2 – Build JoinBlock Pipeline

**Key Concepts:**
- Create three `TransformBlock<string, T>` instances (one for each data type)
- Create one `JoinBlock<Weather, AirQuality, Traffic>` to synchronize results
- Link each TransformBlock to the corresponding JoinBlock target (Target1, Target2, Target3)
- Create an `ActionBlock<Tuple<Weather, AirQuality, Traffic>>` to process results
- Use `PropagateCompletion = true` in LinkTo options

**Pipeline Structure:**
```
[TransformBlock<string, Weather>] ──→ JoinBlock.Target1
[TransformBlock<string, AirQuality>] ──→ JoinBlock.Target2  ──→ [JoinBlock] ──→ [ActionBlock]
[TransformBlock<string, Traffic>] ──→ JoinBlock.Target3
```

**ActionBlock processing:**
- Deconstruct the tuple: `var (weather, airQuality, traffic) = tuple;`
- Print the unified report with all three data types
- Format: City name, Weather, Air Quality, Traffic

**Completion flow:**
- Each source block needs to be marked Complete()
- PropagateCompletion ensures downstream blocks complete automatically
- Await the final ActionBlock.Completion

---

### TODO N3 – Process Cities and Verify Synchronization

**Key Concepts:**
- Post each city name to ALL THREE TransformBlock inputs
- This ensures JoinBlock receives matching items from each stream
- Use a loop to post cities: `foreach (var city in cities)`
- After posting all cities, call `.Complete()` on each source TransformBlock
- Finally, `await reportBlock.Completion` to wait for all processing

**Critical detail:** Each city must be posted to weatherBlock, airQualityBlock, AND trafficBlock so JoinBlock can synchronize them.

---

## Common Pitfalls

❌ **Forgetting to post to all three blocks:**
```csharp
// Wrong - only posts to one block
weatherBlock.Post("Seattle");
```

✅ **Correct - posts to all three:**
```csharp
weatherBlock.Post("Seattle");
airQualityBlock.Post("Seattle");
trafficBlock.Post("Seattle");
```

❌ **Not completing all source blocks:**
```csharp
// Wrong - JoinBlock will hang waiting for more input
weatherBlock.Complete();
// Missing: airQualityBlock.Complete() and trafficBlock.Complete()
```

❌ **Wrong JoinBlock target assignment:**
```csharp
// Wrong - types don't match
weatherBlock.LinkTo(joinBlock.Target2);  // Target2 expects AirQuality, not Weather
```

---

<details>
<summary>Reference Solution (open after completion)</summary>

```csharp
using System.Threading.Tasks.Dataflow;

namespace Lab.Iter05;

// Data models
public record Weather(string City, int Temperature, string Condition);
public record AirQuality(string City, int AQI, string Level);
public record Traffic(string City, string Status);

public class WeatherDashboard
{
    // Mock data sources
    private static readonly Dictionary<string, (int temp, string condition)> WeatherData = new()
    {
        ["Seattle"] = (52, "Rainy"),
        ["Portland"] = (58, "Cloudy"),
        ["San Francisco"] = (65, "Sunny")
    };

    private static readonly Dictionary<string, (int aqi, string level)> AirQualityData = new()
    {
        ["Seattle"] = (45, "Good"),
        ["Portland"] = (78, "Moderate"),
        ["San Francisco"] = (55, "Good")
    };

    private static readonly Dictionary<string, string> TrafficData = new()
    {
        ["Seattle"] = "Moderate",
        ["Portland"] = "Light",
        ["San Francisco"] = "Heavy"
    };

    // TODO[N1]: Create Weather Data Fetchers
    public static async Task<Weather> FetchWeatherAsync(string city)
    {
        await Task.Delay(100);
        var data = WeatherData[city];
        return new Weather(city, data.temp, data.condition);
    }

    public static async Task<AirQuality> FetchAirQualityAsync(string city)
    {
        await Task.Delay(150);
        var data = AirQualityData[city];
        return new AirQuality(city, data.aqi, data.level);
    }

    public static async Task<Traffic> FetchTrafficAsync(string city)
    {
        await Task.Delay(80);
        return new Traffic(city, TrafficData[city]);
    }

    // TODO[N2]: Build JoinBlock Pipeline
    public static async Task RunDashboardAsync(List<string> cities)
    {
        // Create TransformBlocks for each data source
        var weatherBlock = new TransformBlock<string, Weather>(
            async city => await FetchWeatherAsync(city)
        );

        var airQualityBlock = new TransformBlock<string, AirQuality>(
            async city => await FetchAirQualityAsync(city)
        );

        var trafficBlock = new TransformBlock<string, Traffic>(
            async city => await FetchTrafficAsync(city)
        );

        // Create JoinBlock to synchronize all three streams
        var joinBlock = new JoinBlock<Weather, AirQuality, Traffic>();

        // Link TransformBlocks to JoinBlock targets
        weatherBlock.LinkTo(joinBlock.Target1, new DataflowLinkOptions { PropagateCompletion = true });
        airQualityBlock.LinkTo(joinBlock.Target2, new DataflowLinkOptions { PropagateCompletion = true });
        trafficBlock.LinkTo(joinBlock.Target3, new DataflowLinkOptions { PropagateCompletion = true });

        // Create ActionBlock to process joined results
        var reportBlock = new ActionBlock<Tuple<Weather, AirQuality, Traffic>>(tuple =>
        {
            var (weather, airQuality, traffic) = tuple;
            Console.WriteLine("\n=== City Status Report ===");
            Console.WriteLine($"City: {weather.City}");
            Console.WriteLine($"  Weather: {weather.Temperature}°F, {weather.Condition}");
            Console.WriteLine($"  Air Quality: {airQuality.Level} ({airQuality.AQI} AQI)");
            Console.WriteLine($"  Traffic: {traffic.Status}");
        });

        // Link JoinBlock to ActionBlock
        joinBlock.LinkTo(reportBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // TODO[N3]: Process Cities and Verify Synchronization
        foreach (var city in cities)
        {
            weatherBlock.Post(city);
            airQualityBlock.Post(city);
            trafficBlock.Post(city);
        }

        // Complete source blocks
        weatherBlock.Complete();
        airQualityBlock.Complete();
        trafficBlock.Complete();

        // Wait for pipeline to complete
        await reportBlock.Completion;
    }
}
```

</details>
