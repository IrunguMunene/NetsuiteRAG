using System.Collections.Concurrent;

namespace NetSuiteRAG.Shared.Monitoring;

/// <summary>
/// Simple in-memory metrics collector for basic observability
/// </summary>
public class MetricsCollector
{
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, List<double>> _histograms = new();
    private readonly object _lock = new();

    public void IncrementCounter(string name, long value = 1)
    {
        _counters.AddOrUpdate(name, value, (_, current) => current + value);
    }

    public void RecordLatency(string operation, double milliseconds)
    {
        _histograms.AddOrUpdate(
            operation,
            _ => new List<double> { milliseconds },
            (_, list) =>
            {
                lock (_lock)
                {
                    list.Add(milliseconds);
                    // Keep only last 1000 samples
                    if (list.Count > 1000)
                    {
                        list.RemoveAt(0);
                    }
                }
                return list;
            });
    }

    public Dictionary<string, object> GetMetricsSummary()
    {
        var summary = new Dictionary<string, object>();

        // Add counters
        foreach (var counter in _counters)
        {
            summary[$"counter.{counter.Key}"] = counter.Value;
        }

        // Add histogram stats
        foreach (var histogram in _histograms)
        {
            lock (_lock)
            {
                if (histogram.Value.Count > 0)
                {
                    var values = histogram.Value.ToList();
                    values.Sort();

                    summary[$"latency.{histogram.Key}.count"] = values.Count;
                    summary[$"latency.{histogram.Key}.avg"] = values.Average();
                    summary[$"latency.{histogram.Key}.min"] = values.Min();
                    summary[$"latency.{histogram.Key}.max"] = values.Max();
                    summary[$"latency.{histogram.Key}.p50"] = GetPercentile(values, 0.50);
                    summary[$"latency.{histogram.Key}.p95"] = GetPercentile(values, 0.95);
                    summary[$"latency.{histogram.Key}.p99"] = GetPercentile(values, 0.99);
                }
            }
        }

        return summary;
    }

    private static double GetPercentile(List<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0) return 0;

        var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
        index = Math.Max(0, Math.Min(sortedValues.Count - 1, index));

        return sortedValues[index];
    }

    public void Reset()
    {
        _counters.Clear();
        _histograms.Clear();
    }
}
