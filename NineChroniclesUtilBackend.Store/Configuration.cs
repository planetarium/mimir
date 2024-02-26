namespace NineChroniclesUtilBackend.Store;

public class Configuration
{
    public string EmptyChronicleBaseUrl { get; init; }
    public string MongoDbConnectionString { get; init; }
    public string DatabaseName { get; set; }
}
