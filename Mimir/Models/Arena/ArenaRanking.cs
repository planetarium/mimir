using System.Text.Json.Serialization;

namespace Mimir.Models.Arena;

public class ArenaRanking(
    string avatarAddress,
    string arenaAddress,
    int win,
    int lose,
    long rank,
    int ticket,
    int ticketResetCount,
    int purchasedTicketCount,
    int score
)
{
    public string AvatarAddress { get; set; } = avatarAddress;
    public string ArenaAddress { get; set; } = arenaAddress;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CP { get; set; }
    public int Win { get; set; } = win;
    public int Lose { get; set; } = lose;
    public long Rank { get; set; } = rank;
    public int Ticket { get; set; } = ticket;
    public int TicketResetCount { get; set; } = ticketResetCount;
    public int PurchasedTicketCount { get; set; } = purchasedTicketCount;
    public int Score { get; set; } = score;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Avatar? Avatar { get; set; }
}
