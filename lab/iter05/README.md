# Lab Iter05: Multi-Source Weather Dashboard

## Overview

Build a coordinated data pipeline that synchronizes weather information from three independent data sources using `JoinBlock<T1, T2, T3>`. Your dashboard will fetch weather data, air quality index, and traffic conditions for cities, then join them into unified status reports.

**Learning Objectives:**

- Use JoinBlock to synchronize multiple input streams
- Understand multi-input target interfaces (Target1, Target2, Target3)
- Coordinate parallel async operations with proper completion flow
- Apply FIFO ordering guarantees across inputs

---

## Scenario

You're building a city dashboard that needs to display:

1. **Weather** - Temperature and conditions
2. **Air Quality Index (AQI)** - Pollution level
3. **Traffic Status** - Current congestion level

Each data source is independent and returns results at different speeds. You must coordinate all three sources so that data for the same city is properly synchronized.

---

## TODO N1 – Create Weather Data Fetchers

**Objective:** Implement three async methods that simulate fetching data from different APIs, each with realistic delays.

**Requirements:**

- `FetchWeatherAsync(string city)` - Returns weather with 100ms delay
- `FetchAirQualityAsync(string city)` - Returns AQI with 150ms delay  
- `FetchTrafficAsync(string city)` - Returns traffic status with 80ms delay
- Use provided data dictionaries for results
- Add `await Task.Delay()` to simulate network latency

**Hints:**

- Each method should look up the city in its respective dictionary
- Return a record/struct with the city name and data
- The delays simulate real-world API response times

## TODO N2 – Build JoinBlock Pipeline

**Objective:** Create a JoinBlock-based pipeline that coordinates the three data sources and produces unified city reports.

**Requirements:**

- Create three `TransformBlock` instances (one per data source)
- Create a `JoinBlock<Weather, AirQuality, Traffic>`
- Link each TransformBlock to the corresponding JoinBlock target
- Create an `ActionBlock` that receives tuples and prints unified reports
- Implement proper completion propagation using `PropagateCompletion = true`

**Hints:**

- Each TransformBlock calls one of your fetch methods
- JoinBlock.Target1, Target2, Target3 receive different data types
- Link the JoinBlock output to the ActionBlock
- Complete the pipeline by calling Complete() on all source blocks

## TODO N3 – Process Cities and Verify Synchronization

**Objective:** Post city names to all three source blocks and verify that results are properly synchronized.

**Requirements:**

- Post 3 cities: "Seattle", "Portland", "San Francisco"
- Ensure each city name goes to ALL three TransformBlock inputs
- Wait for complete pipeline completion
- Verify output shows synchronized data (same city together in each report)

**Hints:**

- Post each city to all three TransformBlock inputs
- Call Complete() on each source TransformBlock
- Await the final ActionBlock.Completion
- The JoinBlock will automatically pair results by arrival order (FIFO per input)

## Expected Output

```
=== City Status Report ===
City: Seattle
  Weather: 52°F, Rainy
  Air Quality: Good (45 AQI)
  Traffic: Moderate

=== City Status Report ===
City: Portland
  Weather: 58°F, Cloudy
  Air Quality: Moderate (78 AQI)
  Traffic: Light

=== City Status Report ===
City: San Francisco
  Weather: 65°F, Sunny
  Air Quality: Good (55 AQI)
  Traffic: Heavy
```

---

## Running the Lab

```bash
cd lab/iter05
dotnet run
```

All tests should pass once you've correctly implemented the TODOs.