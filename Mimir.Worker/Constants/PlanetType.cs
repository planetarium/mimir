namespace Mimir.Worker.Constants;

public class PlanetType
{
    private PlanetType(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static PlanetType ODIN => new("odin");

    public static PlanetType HEIMDALL => new("heimdall");

    public static PlanetType FromString(string planetType)
    {
        return planetType switch
        {
            null => throw new ArgumentNullException(nameof(planetType)),
            "odin" => new PlanetType("odin"),
            "heimdall" => new PlanetType("heimdall"),
            _ => throw new ArgumentException($"Not expected planetType. {planetType}")
        };
    }

    public override string ToString()
    {
        return Value;
    }
}
