namespace Serilog.Sinks.Axiom;

public class AxiomConfiguration
{
    public List<string> Removals { get; set; } = new();
    public Dictionary<string, string> Renames { get; set; } = new();

    public AxiomConfiguration() { }

    public AxiomConfiguration(List<string> removes, Dictionary<string, string> renames)
    {
        Removals = removes;
        Renames = renames;
    }
}
