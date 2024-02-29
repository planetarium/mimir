using NineChroniclesUtilBackend.Models.Agent;

namespace NineChroniclesUtilBackend.Models.Arena;

public class ArenaRanking(
    string AvatarAddress,
    string ArenaAddress,
    int Cp,
    int Win,
    int Lose,
    long Rank,
    int Ticket,
    int TicketResetCount,
    int PurchasedTicketCount,
    int Score,
    Avatar Avatar
)
{
    public string AvatarAddress { get; set; } = AvatarAddress;
    public string ArenaAddress { get; set; } = ArenaAddress;
    public int Cp { get; set; } = Cp;
    public int Win { get; set; } = Win;
    public int Lose { get; set; } = Lose;
    public long Rank { get; set; } = Rank;
    public int Ticket { get; set; } = Ticket;
    public int TicketResetCount { get; set; } = TicketResetCount;
    public int PurchasedTicketCount { get; set; } = PurchasedTicketCount;
    public int Score { get; set; } = Score;
    public Avatar Avatar { get; set; } = Avatar;
}
