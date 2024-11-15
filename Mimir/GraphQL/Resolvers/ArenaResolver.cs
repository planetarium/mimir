using HotChocolate.Resolvers;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
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

        var collectionName = CollectionNames.GetCollectionName<ArenaParticipantDocument>();
        var metadataDocument = await metadataRepo.GetByCollectionAsync(collectionName);
        arenaRound = tableSheetsRepo.GetArenaRound(metadataDocument.LatestBlockIndex);
        context.ScopedContextData = context.ScopedContextData.Add("arenaRound", arenaRound);
        return arenaRound;
    }

    public static async Task<List<ArenaParticipantDocument>> GetLeaderboardAsync(
        IResolverContext context,
        int ranking,
        int length,
        [Service] ArenaParticipantRepository arenaParticipantRepository,
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

        var collectionName = CollectionNames.GetCollectionName<ArenaParticipantDocument>();
        var metadata = await metadataRepo.GetByCollectionAsync(collectionName);
        arenaRound ??= await GetRoundAsync(context, metadataRepo, tableSheetsRepo, arenaRound);
        return await arenaParticipantRepository.GetLeaderboardAsync(
            metadata.LatestBlockIndex,
            arenaRound.ChampionshipId,
            arenaRound.Round,
            ranking,
            length);
    }

    public static async Task<int?> GetRankingAsync(
        IResolverContext context,
        Address avatarAddress,
        [Service] ArenaParticipantRepository arenaParticipantRepository,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        var collectionName = CollectionNames.GetCollectionName<ArenaParticipantDocument>();
        var metadata = await metadataRepo.GetByCollectionAsync(collectionName);
        arenaRound ??= await GetRoundAsync(context, metadataRepo, tableSheetsRepo, arenaRound);
        return await arenaParticipantRepository.GetRankingByAddressAsync(
            metadata.LatestBlockIndex,
            arenaRound.ChampionshipId,
            arenaRound.Round,
            avatarAddress);
    }

    public static async Task<List<ArenaParticipantDocument>> GetLeaderboardByAvatarAddressAsync(
        IResolverContext context,
        Address avatarAddress,
        [Service] ArenaParticipantRepository arenaParticipantRepository,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [ScopedState("arenaRound")] ArenaSheet.RoundData? arenaRound)
    {
        var collectionName = CollectionNames.GetCollectionName<ArenaParticipantDocument>();
        var metadata = await metadataRepo.GetByCollectionAsync(collectionName);
        arenaRound ??= await GetRoundAsync(context, metadataRepo, tableSheetsRepo, arenaRound);
        return await arenaParticipantRepository.GetLeaderboardByAvatarAddressAsync(
            metadata.LatestBlockIndex,
            arenaRound.ChampionshipId,
            arenaRound.Round,
            arenaRound.ArenaType,
            avatarAddress);
    }
}
