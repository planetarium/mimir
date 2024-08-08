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

    public static PlanetType FromString(string planetType)
    {
        return planetType switch
        {
            "odin" => new PlanetType("odin"),
            "heimdall" => new PlanetType("heimdall"),
            _ => throw new ArgumentException("Not expected planetType")
        };
    }

    public override string ToString()
    {
        return Value;
    }
}
