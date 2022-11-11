namespace Serilog.Sinks.Axiom;

public class AxiomConfiguration
{
    public string Token { get; set; }
    public string OrgID { get; set; }
    public string Dataset { get; set; }

    public AxiomConfiguration() : this("", "", "") { }

    public AxiomConfiguration(string token, string orgID, string dataset)
    {
        Token = token;
        OrgID = orgID;
        Dataset = dataset;
    }
}
