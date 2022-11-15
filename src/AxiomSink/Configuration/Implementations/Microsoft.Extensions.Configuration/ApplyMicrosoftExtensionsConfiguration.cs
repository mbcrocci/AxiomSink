using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.Axiom;

internal static class ApplyMicrosoftExtensionsConfiguration
{
    internal static AxiomConfiguration ConfigureAxiomConfiguration(
        AxiomConfiguration? axiomConfiguration,
        IConfigurationSection? configurationSection)
    {
        if (configurationSection == null || !configurationSection.GetChildren().Any())
            return axiomConfiguration ?? new AxiomConfiguration();

        var section = configurationSection.Get<AxiomConfiguration>();

        return new AxiomConfiguration(
            removes: axiomConfiguration?.Removals ?? section.Removals,
            renames: axiomConfiguration?.Renames ?? section.Renames);
    }
}
