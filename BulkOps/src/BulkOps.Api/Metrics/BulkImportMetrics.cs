using System.Diagnostics.Metrics;

namespace BulkOps.Api.Metrics;

public sealed class BulkImportMetrics : IDisposable
{
    public const string MeterName = "BulkOps.Api";

    private readonly Meter _meter;
    private readonly Histogram<double> _importDuration;
    private readonly Counter<long> _importedUsers;

    public BulkImportMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        _importDuration = _meter.CreateHistogram<double>(
            "bulkops.import.duration",
            unit: "s",
            description: "Duration of a bulk user import operation in seconds.");

        _importedUsers = _meter.CreateCounter<long>(
            "bulkops.import.users",
            unit: "{user}",
            description: "Total number of users inserted during bulk import operations.");
    }

    public void RecordImport(double durationSeconds, int userCount)
    {
        _importDuration.Record(durationSeconds);
        _importedUsers.Add(userCount);
    }

    public void Dispose() => _meter.Dispose();
}
