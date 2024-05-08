using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Mimir.Arena;
using Mimir.Models.Arena;
using Mimir.Repositories;
using Mimir.Services;
using Mimir.Util;
using Nekoyume.TableData;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/arena")]
public class ArenaController(ArenaRankingRepository arenaRankingRepository) : ControllerBase
{
    [HttpGet("ranking/{avatarAddress}/rank")]
    public async Task<long> GetRankByAvatarAddress(string network, string avatarAddress)
    {
        var rank = await arenaRankingRepository.GetRankByAvatarAddress(network, avatarAddress);
        if (rank == 0)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return default;
        }

        return rank;
    }

    [HttpGet("ranking")]
    public async Task<List<ArenaRanking>> GetRanking(string network, int limit, int offset)
    {
        return await arenaRankingRepository.GetRanking(network, limit, offset);
    }

    [HttpPost("simulate")]
    public async Task<ArenaSimulateResponse> Simulate(
        string network,
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
        var enemyAvatarState = await stateGetter.GetAvatarStateAsync(enemyAvatarAddress);
        var enemyAvatarItemSlotState = await stateGetter.GetItemSlotStateAsync(enemyAvatarAddress);
        var enemyAvatarRuneStates = await stateGetter.GetRuneStatesAsync(enemyAvatarAddress);

        var bulkSimulator = new ArenaBulkSimulator();
        var result = await bulkSimulator.BulkSimulate(
            new AvatarStatesForArena(myAvatarState, myAvatarItemSlotState, myAvatarRuneStates),
            new AvatarStatesForArena(
                enemyAvatarState,
                enemyAvatarItemSlotState,
                enemyAvatarRuneStates
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
                await stateGetter.GetSheetAsync<RuneOptionSheet>()
            ),
            await stateGetter.GetCollectionStatesAsync([myAvatarAddress, enemyAvatarAddress]),
            await stateGetter.GetSheetAsync<CollectionSheet>(),
            await stateGetter.GetSheetAsync<DeBuffLimitSheet>()
        );

        return new ArenaSimulateResponse(result);
    }
}
