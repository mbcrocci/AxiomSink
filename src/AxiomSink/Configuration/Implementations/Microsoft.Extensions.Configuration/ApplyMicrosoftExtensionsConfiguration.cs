using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.Axiom;

internal static class ApplyMicrosoftExtensionsConfiguration
{
    internal static AxiomConfiguration ConfigureAxiomConfiguration(
        AxiomConfiguration axiomConfiguration,
        IConfigurationSection configurationSection)
    {
        if (configurationSection == null || !configurationSection.GetChildren().Any())
            return axiomConfiguration ?? new AxiomConfiguration();

        var section  = configurationSection.Get<AxiomConfiguration>();

        return new AxiomConfiguration(
            token: axiomConfiguration?.Token ?? section.Token,
            orgID: axiomConfiguration?.OrgID ?? section.OrgID,
            dataset: axiomConfiguration?.Dataset ?? section.Dataset);
    }
}
