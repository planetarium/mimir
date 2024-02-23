using Nekoyume.Model.Arena;

namespace NineChroniclesUtilBackend.Store.Models;

public class ArenaData
{
    public ArenaScore Score { get; }
    public ArenaInformation Information { get; }

    public ArenaData(ArenaScore score, ArenaInformation information)
    {
        Score = score;
        Information = information;
    }
}