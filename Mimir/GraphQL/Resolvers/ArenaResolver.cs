using HotChocolate.Resolvers;
using Lib9c.GraphQL.Enums;
using Libplanet.Crypto;
using Mimir.Repositories;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Resolvers;

public class ArenaResolver
{
    public static ArenaSheet.RoundData GetArenaRound(
        IResolverContext context,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        if (arenaRound is not null)
        {
            return arenaRound;
        }

        var latestBlockIndex = metadataRepo.GetLatestBlockIndex(planetName, "BlockPoller");
        arenaRound = tableSheetsRepo.GetArenaRound(planetName, latestBlockIndex);
        context.ScopedContextData = context.ScopedContextData.Add("arenaRound", arenaRound);
        return arenaRound;
    }

    public static async Task<long?> GetRanking(
        IResolverContext context,
        Address avatarAddress,
        [Service] ArenaRankingRepository arenaRankingRepo,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        arenaRound ??= GetArenaRound(context, metadataRepo, tableSheetsRepo, planetName, arenaRound);
        var rank = await arenaRankingRepo.GetRankingByAvatarAddressAsync(
            planetName,
            avatarAddress,
            arenaRound.ChampionshipId,
            arenaRound.Round);
        return rank == 0
            ? null
            : rank;
    }

    public static int GetRound(
        IResolverContext context,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("planetName")] PlanetName planetName,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound) =>
        GetArenaRound(context, metadataRepo, tableSheetsRepo, planetName, arenaRound).Round;

    // public static int? GetRank(
    //     IResolverContext context,
    //     [Service] MetadataRepository metadataRepo,
    //     [Service] TableSheetsRepository tableSheetsRepo,
    //     [ScopedState("planetName")] PlanetName planetName,
    //     [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound) =>
    //     GetArenaRound(context, metadataRepo, tableSheetsRepo, planetName, arenaRound)?.round;
}
