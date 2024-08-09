using System.Text.Json.Serialization;

namespace Mimir.Models.Arena;

public record ArenaRanking(
    string AvatarAddress, // FIXME: to Address type
    string ArenaAddress, // FIXME: to Address type
    int Win,
    int Lose,
    long Rank,
    int Ticket,
    int TicketResetCount,
    int PurchasedTicketCount,
    int Score)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Avatar? Avatar { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CP { get; set; }
}
