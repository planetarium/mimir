using System.Text.Json.Serialization;
using NineChroniclesUtilBackend.Models.Agent;

namespace NineChroniclesUtilBackend.Models.Arena;

public class ArenaRanking(
    string AvatarAddress,
    string ArenaAddress,
    int Win,
    int Lose,
    long Rank,
    int Ticket,
    int TicketResetCount,
    int PurchasedTicketCount,
    int Score
)
{
    public string AvatarAddress { get; set; } = AvatarAddress;
    public string ArenaAddress { get; set; } = ArenaAddress;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CP { get; set; }
    public int Win { get; set; } = Win;
    public int Lose { get; set; } = Lose;
    public long Rank { get; set; } = Rank;
    public int Ticket { get; set; } = Ticket;
    public int TicketResetCount { get; set; } = TicketResetCount;
    public int PurchasedTicketCount { get; set; } = PurchasedTicketCount;
    public int Score { get; set; } = Score;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Avatar? Avatar { get; set; }
}
