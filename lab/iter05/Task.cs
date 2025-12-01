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
    // Implement three async methods that simulate fetching data from different APIs
    public static async Task<Weather> FetchWeatherAsync(string city)
    {
        // [YOUR CODE GOES HERE]
        _ = WeatherData.TryGetValue(city, out var weather);
        var result = new Weather(city, weather.temp, weather.condition);
        await Task.Delay(100).ConfigureAwait(false);
        return result;
    }

    public static async Task<AirQuality> FetchAirQualityAsync(string city)
    {
        // [YOUR CODE GOES HERE]
        _ = AirQualityData.TryGetValue(city, out var quality);
        var result = new AirQuality(city, quality.aqi, quality.level);
        await Task.Delay(150).ConfigureAwait(false);
        return result;
    }

    public static async Task<Traffic> FetchTrafficAsync(string city)
    {
        // [YOUR CODE GOES HERE]
        _ = TrafficData.TryGetValue(city, out var status);
        var result = new Traffic(city, status);
        await Task.Delay(80).ConfigureAwait(false);
        return result;
    }

    // TODO[N2]: Build JoinBlock Pipeline
    // Create a JoinBlock-based pipeline that coordinates the three data sources
    public static async Task RunDashboardAsync(List<string> cities)
    {
        // plan:
        // Input BufferBlock
        // BroadcastBlock to 3 blocks for weather, traffic and air (each are TransformBlock)
        // Each above transform block join in JoinBlock
        // JoinBlock fed into Action block which output to console the following for each item:
        // === City Status Report ===
        // City: Seattle
        // Weather: 52°F, Rainy
        // Air Quality: Good (45 AQI)
        // Traffic: Moderate

        var serviceBlockOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 1024,
            EnsureOrdered = true,
            MaxDegreeOfParallelism = 3
        };

        var input = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 1024, EnsureOrdered = false });
        var broadcast = new BroadcastBlock<string>(s => s, new DataflowBlockOptions { BoundedCapacity = 1024, EnsureOrdered = false });
        var weather = new TransformBlock<string, Weather>(FetchWeatherAsync, serviceBlockOptions);
        var air = new TransformBlock<string, AirQuality>(FetchAirQualityAsync, serviceBlockOptions);
        var traffic = new TransformBlock<string, Traffic>(FetchTrafficAsync, serviceBlockOptions);
        var join = new JoinBlock<Weather, AirQuality, Traffic>(new GroupingDataflowBlockOptions { Greedy = true, BoundedCapacity = 1024 });
        var output = new ActionBlock<Tuple<Weather, AirQuality, Traffic>>(data =>
        {
            var weather = data.Item1;
            var air = data.Item2;
            var traffic = data.Item3;

            Console.WriteLine("\n=== City Status Report ===");
            Console.WriteLine($"City: {weather.City}");
            Console.WriteLine($"  Weather: {weather.Temperature}°F, {weather.Condition}");
            Console.WriteLine($"  Air Quality: {air.Level} ({air.AQI} AQI)");
            Console.WriteLine($"  Traffic: {traffic.Status}");
            Console.WriteLine();
        }, new ExecutionDataflowBlockOptions { BoundedCapacity = 1024, MaxDegreeOfParallelism = 1, EnsureOrdered = false });

        var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
        input.LinkTo(broadcast, linkOpts);
        broadcast.LinkTo(weather, linkOpts);
        broadcast.LinkTo(air, linkOpts);
        broadcast.LinkTo(traffic, linkOpts);
        weather.LinkTo(join.Target1, linkOpts);
        air.LinkTo(join.Target2, linkOpts);
        traffic.LinkTo(join.Target3, linkOpts);
        join.LinkTo(output, linkOpts);

        foreach (var e in cities)
        {
            await input.SendAsync(e).ConfigureAwait(false);
        }

        input.Complete();
        await output.Completion.ConfigureAwait(false);
    }
}