using Libplanet.Crypto;

namespace Mimir.Store.Models;

public class ScrapperResult
{
    public DateTime StartTime { get; set; }
    public int TotalElapsedMinutes { get; set; }
    public int AvatarScrappedCount { get; set; }
    public int ArenaScrappedCount { get; set; }
    public List<Address> FailedAvatarAddresses { get; } = new List<Address>();
    public List<Address> FailedArenaAddresses { get; } = new List<Address>();

    public override string ToString()
    {
        var failedAvatarAddresses = string.Join(", ", FailedAvatarAddresses.Select(a => a.ToString()));
        var failedArenaAddresses = string.Join(", ", FailedArenaAddresses.Select(a => a.ToString()));

        return $"StartTime: {StartTime}, " +
                $"TotalElapsedMinutes: {TotalElapsedMinutes}, " +
                $"AvatarScrappedCount: {AvatarScrappedCount}, " +
                $"ArenaScrappedCount: {ArenaScrappedCount}, " +
                $"FailedAvatarAddresses: [{failedAvatarAddresses}], " +
                $"FailedArenaAddresses: [{failedArenaAddresses}]";
    }
}
