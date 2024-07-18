using HotChocolate.Resolvers;
using Libplanet.Crypto;
using Mimir.Models.Arena;
using Mimir.Repositories;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Resolvers;

public class ArenaResolver
{
    public static ArenaSheet.RoundData GetArenaRound(
        IResolverContext context,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        if (arenaRound is not null)
        {
            return arenaRound;
        }

        var latestBlockIndex = metadataRepo.GetLatestBlockIndex("BlockPoller");
        arenaRound = tableSheetsRepo.GetArenaRound(latestBlockIndex);
        context.ScopedContextData = context.ScopedContextData.Add("arenaRound", arenaRound);
        return arenaRound;
    }

    public static async Task<long?> GetRanking(
        IResolverContext context,
        Address avatarAddress,
        [Service] ArenaRepository arenaRankingRepo,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        arenaRound ??= GetArenaRound(context, metadataRepo, tableSheetsRepo, arenaRound);
        var rank = await arenaRankingRepo.GetRankingByAvatarAddressAsync(
            avatarAddress,
            arenaRound.ChampionshipId,
            arenaRound.Round);
        return rank == 0
            ? null
            : rank;
    }

    public static async Task<List<ArenaRanking>> GetLeaderboard(
        IResolverContext context,
        long ranking,
        int length,
        [Service] ArenaRepository arenaRankingRepo,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        if (ranking < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ranking),
                "This must be greater than or equal to 1.");
        }

        switch (length)
        {
            case < 1:
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "This must be greater than or equal to 1.");
            case > 100:
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "This must be less than or equal to 100.");
        }

        arenaRound ??= GetArenaRound(context, metadataRepo, tableSheetsRepo, arenaRound);
        return await arenaRankingRepo.GetRanking(
            ranking - 1,
            length,
            arenaRound.ChampionshipId,
            arenaRound.Round
        );
    }
}
