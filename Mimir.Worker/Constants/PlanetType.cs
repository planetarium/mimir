namespace Mimir.Worker.Constants;

public class PlanetType
{
    private PlanetType(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static PlanetType ODIN
    {
        get { return new PlanetType("odin"); }
    }
    public static PlanetType HEIMDALL
    {
        get { return new PlanetType("heimdall"); }
    }

    public override string ToString()
    {
        return Value;
    }
}
