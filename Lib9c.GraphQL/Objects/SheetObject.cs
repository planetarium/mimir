namespace Lib9c.GraphQL.Objects;

public class SheetObject
{
    public string Name { get; set; } = string.Empty;
    // public Address Address { get; set; } = default;
    public string? Csv { get; set; }
    public string? Json { get; set; }
}
