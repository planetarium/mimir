using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mimir.Arena;
using Mimir.Models.Arena;
using Mimir.Repositories;
using Mimir.Services;
using Mimir.Util;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;
using Nekoyume.TableData.Rune;

namespace Mimir.Controllers;

[ApiController]
[Route("arena")]
public class ArenaController(
    ArenaRankingRepository arenaRankingRepository,
    TableSheetsRepository tableSheetsRepository
) : ControllerBase
{
    [HttpGet("season")]
    public ArenaSeason GetLatestSeason(
        MetadataRepository metadataRepository
    )
    {
        var arenaRound = tableSheetsRepository.GetArenaRound(
            metadataRepository.GetLatestBlockIndex("BlockPoller")
        );

        return new ArenaSeason(arenaRound.ChampionshipId, arenaRound.Round);
    }

    [HttpGet("ranking/{avatarAddress}/rank")]
    public async Task<long> GetRankByAvatarAddress(
        string avatarAddress,
        [BindRequired] int championshipId,
        [BindRequired] int round
    )
    {
        var rank = await arenaRankingRepository.GetRankingByAvatarAddressAsync(
            new Address(avatarAddress),
            championshipId,
            round
        );
        if (rank == 0)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return default;
        }

        return rank;
    }

    [HttpGet("ranking")]
    public async Task<List<ArenaRanking>> GetRanking(
        [BindRequired] int limit,
        [BindRequired] int offset,
        [BindRequired] int championshipId,
        [BindRequired] int round
    )
    {
        return await arenaRankingRepository.GetRanking(
            offset,
            limit,
            championshipId,
            round
        );
    }

    [HttpPost("simulate")]
    public async Task<ArenaSimulateResponse> Simulate(
        [FromBody] ArenaSimulateRequest arenaSimulateRequest,
        IStateService stateService
    )
    {
        var stateGetter = new StateGetter(stateService);

        var myAvatarAddress = new Address(arenaSimulateRequest.MyAvatarAddress);
        var enemyAvatarAddress = new Address(arenaSimulateRequest.EnemyAvatarAddress);
        var myAvatarState = await stateGetter.GetAvatarStateAsync(myAvatarAddress);
        var myAvatarItemSlotState = await stateGetter.GetItemSlotStateAsync(myAvatarAddress);
        var myAvatarRuneStates = await stateGetter.GetRuneStatesAsync(myAvatarAddress);
        var myAvatarRuneSlotState = await stateGetter.GetRuneSlotStateAsync(
            myAvatarAddress,
            BattleType.Arena
        );
        var enemyAvatarState = await stateGetter.GetAvatarStateAsync(enemyAvatarAddress);
        var enemyAvatarItemSlotState = await stateGetter.GetItemSlotStateAsync(enemyAvatarAddress);
        var enemyAvatarRuneStates = await stateGetter.GetRuneStatesAsync(enemyAvatarAddress);
        var enemyAvatarRuneSlotState = await stateGetter.GetRuneSlotStateAsync(
            enemyAvatarAddress,
            BattleType.Arena
        );

        var bulkSimulator = new ArenaBulkSimulator();
        var result = await bulkSimulator.BulkSimulate(
            new AvatarStatesForArena(
                myAvatarState,
                myAvatarItemSlotState,
                myAvatarRuneStates,
                myAvatarRuneSlotState
            ),
            new AvatarStatesForArena(
                enemyAvatarState,
                enemyAvatarItemSlotState,
                enemyAvatarRuneStates,
                enemyAvatarRuneSlotState
            ),
            new ArenaSimulatorSheets(
                await stateGetter.GetSheetAsync<MaterialItemSheet>(),
                await stateGetter.GetSheetAsync<SkillSheet>(),
                await stateGetter.GetSheetAsync<SkillBuffSheet>(),
                await stateGetter.GetSheetAsync<StatBuffSheet>(),
                await stateGetter.GetSheetAsync<SkillActionBuffSheet>(),
                await stateGetter.GetSheetAsync<ActionBuffSheet>(),
                await stateGetter.GetSheetAsync<CharacterSheet>(),
                await stateGetter.GetSheetAsync<CharacterLevelSheet>(),
                await stateGetter.GetSheetAsync<EquipmentItemSetEffectSheet>(),
                await stateGetter.GetSheetAsync<CostumeStatSheet>(),
                await stateGetter.GetSheetAsync<WeeklyArenaRewardSheet>(),
                await stateGetter.GetSheetAsync<RuneOptionSheet>(),
                await stateGetter.GetSheetAsync<RuneListSheet>(),
                await stateGetter.GetSheetAsync<RuneLevelBonusSheet>()
            ),
            await stateGetter.GetCollectionStatesAsync([myAvatarAddress, enemyAvatarAddress]),
            await stateGetter.GetSheetAsync<CollectionSheet>(),
            await stateGetter.GetSheetAsync<DeBuffLimitSheet>(),
            await stateGetter.GetSheetAsync<BuffLinkSheet>()
        );

        return new ArenaSimulateResponse(result);
    }
}
