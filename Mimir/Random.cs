using Libplanet.Action;

namespace Mimir;

public class Random(int seed) : IRandom
{
    private readonly System.Random _random = new(seed);

    public int Next()
    {
        return _random.Next();
    }

    public int Next(int upperBound)
    {
        return _random.Next(upperBound);
    }

    public int Next(int lowerBound, int upperBound)
    {
        return _random.Next(lowerBound, upperBound);
    }

    public void NextBytes(byte[] buffer)
    {
        _random.NextBytes(buffer);
    }

    public int Seed { get; } = seed;
}