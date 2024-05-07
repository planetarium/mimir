using Libplanet.Crypto;

namespace Mimir.Worker.Models;

public class WorkerResult
{
    public DateTime StartTime { get; set; }
    public int TotalElapsedMinutes { get; set; }
    public int StoreArenaRequestCount { get; set; }
    public int StoreAvatarRequestCount { get; set; }
    public int AvatarStoredCount { get; set; }
    public int ArenaStoredCount { get; set; }
    public List<Address> FailedAvatarAddresses { get; } = new List<Address>();
    public List<Address> FailedArenaAddresses { get; } = new List<Address>();

    public override string ToString()
    {
        var failedAvatarAddresses = string.Join(", ", FailedAvatarAddresses.Select(a => a.ToString()));
        var failedArenaAddresses = string.Join(", ", FailedArenaAddresses.Select(a => a.ToString()));

        return $"StartTime: {StartTime}, " +
                $"TotalElapsedMinutes: {TotalElapsedMinutes}, " +
                $"StoreArenaRequestCount: {StoreArenaRequestCount}, " +
                $"StoreAvatarRequestCount: {StoreAvatarRequestCount}, " +
                $"AvatarStoredCount: {AvatarStoredCount}, " +
                $"ArenaStoredCount: {ArenaStoredCount}, " +
                $"FailedAvatarAddresses: [{failedAvatarAddresses}], " +
                $"FailedArenaAddresses: [{failedArenaAddresses}]";
    }
}
