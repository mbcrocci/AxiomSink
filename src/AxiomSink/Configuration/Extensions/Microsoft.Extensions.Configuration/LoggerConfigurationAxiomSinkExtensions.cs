using Serilog.Sinks.Axiom;
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Events;

namespace Serilog;

public static class LoggerConfigurationAxiomSinkExtensions
{
    public static LoggerConfiguration Axiom(
        this LoggerSinkConfiguration loggerConfiguration,
        bool enabled = true,
        string token = "",
        string orgID = "",
        string dataset = "",
        AxiomConfiguration? configuration = null,
        IConfigurationSection? configurationSection = null,
        LogEventLevel logLevel = LevelAlias.Minimum)
    {
        var config = ApplyMicrosoftExtensionsConfiguration.ConfigureAxiomConfiguration(configuration, configurationSection);
        var sink = AxiomSink.Create(enabled, token, orgID, dataset, config);

        return loggerConfiguration.Sink(sink, logLevel);
    }
}
