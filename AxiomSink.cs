using Serilog;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Configuration;
using AxiomNet;
using Serilog.Core;
using Serilog.Debugging;

public class AxiomSink : IBatchedLogEventSink, IDisposable
{
    private readonly Client _axiomClient;
    private readonly string _dataset;

    private static readonly TimeSpan DefaultBatchPeriod = TimeSpan.FromSeconds(2);
    private const int DefaultBatchSizeLimit = 50;

    private readonly LogFormatter _formatter = new LogFormatter();

    private static readonly IngestOptions _options = new IngestOptions
    {
        TimestampField = "timestamp",
    };

    public AxiomSink(string token, string orgID, string dataset)
    {
        HttpClient client = new();

        _axiomClient = new Client(client, accessToken: token, organisationId: orgID);
        _dataset = dataset;
    }

    public static ILogEventSink Create(string token, string orgID, string dataset)
    {
        var sink = new AxiomSink(token, orgID, dataset);

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
            if (!batch.Any()) return;

            List<object> events = batch.Select(_formatter.FormatMessage).ToList();

            await _axiomClient.Datasets
                .IngestEvents(_dataset, events, _options, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            OnException(e);
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private void OnException(Exception e)
    {
        SelfLog.WriteLine("{0}", e.Message);
    }
}

public static class AxiomSinkExtensions
{
    public static LoggerConfiguration Axiom(
        this LoggerSinkConfiguration loggerConfiguration,
        string token, string orgID, string dataset)
    {
        return loggerConfiguration.Sink(AxiomSink.Create(token, orgID, dataset));
    }
}
