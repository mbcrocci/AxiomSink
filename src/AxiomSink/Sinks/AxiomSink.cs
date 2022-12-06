using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using AxiomNet;
using Serilog.Core;
using Serilog.Debugging;

namespace Serilog.Sinks.Axiom;

public class AxiomSink : IBatchedLogEventSink, IDisposable
{
    private readonly bool _enabled;
    private readonly string _dataset;

    private Client? _axiomClient;

    private static readonly TimeSpan DefaultBatchPeriod = TimeSpan.FromSeconds(2);
    private const int DefaultBatchSizeLimit = 50;

    private readonly LogFormatter _formatter = new LogFormatter();

    private static readonly IngestOptions _options = new IngestOptions
    {
        TimestampField = "timestamp",
    };

    public AxiomSink(
        bool enabled,
        string token,
        string orgID,
        string dataset,
        AxiomConfiguration? config = null)
    {
        _enabled = enabled;
        if (_enabled)
        {
            HttpClient client = new();
            _axiomClient = new Client(client, accessToken: token, organisationId: orgID);
        }

        _dataset = dataset;

        if (config != null)
            _formatter = new LogFormatter(config.Removals, config.Renames);
    }

    public static ILogEventSink Create(
        bool enabled,
        string token,
        string orgID,
        string dataset,
        AxiomConfiguration config)
    {
        var sink = new AxiomSink(enabled, token, orgID, dataset, config);

        return new PeriodicBatchingSink(sink, new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = 50,
            Period = TimeSpan.FromSeconds(10),
        });
    }

    public void Dispose() { }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        try
        {
            if (!_enabled || _axiomClient == null) return;

            if (!batch.Any()) return;

            var events = batch
                .Select(_formatter.FormatMessage)
                .ToList();

            await _axiomClient.Datasets
                .IngestEvents(_dataset, events, _options, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            SelfLog.WriteLine("{0}", e);
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;
}
