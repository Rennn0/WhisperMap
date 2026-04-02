using System.Diagnostics.Metrics;

namespace XatiCraft;

/// <summary>
/// </summary>
public class AppMetrics
{
    private readonly Meter _meter;
    private readonly Metric<long> _unhandledExceptionMetric;
    private readonly Metric<float> _funcExecutionMetric;

    /// <summary>
    /// </summary>
    public string Name => _meter.Name;

    /// <summary>
    /// </summary>
    public Counter<long> UnhandledExceptionMetric => (Counter<long>)_unhandledExceptionMetric.Instrument;

    /// <summary>
    /// </summary>
    public Histogram<float> FuncExecutionMetric => (Histogram<float>)_funcExecutionMetric.Instrument;

    /// <summary>
    /// </summary>
    public AppMetrics()
    {
        string instance = Environment.GetEnvironmentVariable("SERVICE_INSTANCE_ID") ?? Environment.MachineName;
        _meter ??= new Meter("xc_api_meter", instance);

        _unhandledExceptionMetric = new Metric<long>
        {
            Key = "0",
            Instrument = _meter.CreateCounter<long>("xc_api_unhandled_exception", null,
                "exceptions thats been captured in catch block, tags include exc detailes")
        };

        _funcExecutionMetric = new Metric<float>
        {
            Key = "1",
            Instrument = _meter.CreateHistogram<float>("xc_api_exec_measurement", "ms",
                "measurements of functions, tags include functions")
        };
    }

    private struct Metric<T> where T : struct
    {
        public string Key { get; set; }
        public Instrument<T> Instrument { get; init; }
    }
}