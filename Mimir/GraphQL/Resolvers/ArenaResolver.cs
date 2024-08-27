using HotChocolate.Resolvers;
using Libplanet.Crypto;
using Mimir.Enums;
using Mimir.MongoDB.Bson;
using Mimir.Repositories;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Resolvers;

public class ArenaResolver
{
    public static async Task<ArenaSheet.RoundData> GetRoundAsync(
        IResolverContext context,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        if (arenaRound is not null)
        {
            return arenaRound;
        }

        var metadataDocument = await metadataRepo.GetByCollectionAsync(CollectionNames.Arena.Value);
        arenaRound = tableSheetsRepo.GetArenaRound(metadataDocument.LatestBlockIndex);
        context.ScopedContextData = context.ScopedContextData.Add("arenaRound", arenaRound);
        return arenaRound;
    }

    public static async Task<long?> GetRankingAsync(
        IResolverContext context,
        Address avatarAddress,
        [Service] ArenaRepository arenaRankingRepo,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        arenaRound ??= await GetRoundAsync(context, metadataRepo, tableSheetsRepo, arenaRound);
        var rank = await arenaRankingRepo.GetRankingByAvatarAddressAsync(
            avatarAddress,
            arenaRound.ChampionshipId,
            arenaRound.Round
        );
        return rank == 0
            ? null
            : rank;
    }

    public static async Task<IEnumerable<ArenaRankingDocument>> GetLeaderboardAsync(
        IResolverContext context,
        int ranking,
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
                "This must be greater than or equal to 1."
            );
        }

        switch (length)
        {
            case < 1:
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "This must be greater than or equal to 1."
                );
            case > 100:
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "This must be less than or equal to 100."
                );
        }

        arenaRound ??= await GetRoundAsync(context, metadataRepo, tableSheetsRepo, arenaRound);
        return await arenaRankingRepo.GetLeaderboardAsync(
            metadataRepo.GetByCollectionAsync(CollectionNames.Arena.Value).Result.LatestBlockIndex,
            arenaRound.ChampionshipId,
            arenaRound.Round,
            ranking - 1,
            length);
    }
}
