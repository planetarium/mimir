namespace Mimir.Worker.Models;

public class BlockResponse
{
    public string Hash { get; }
    public long Index { get; }

    public BlockResponse(string hash, long index)
    {
        Hash = hash;
        Index = index;
    }
}