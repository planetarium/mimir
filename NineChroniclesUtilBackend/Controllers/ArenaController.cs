using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Nekoyume.TableData;
using NineChroniclesUtilBackend.Arena;
using NineChroniclesUtilBackend.Models.Arena;
using NineChroniclesUtilBackend.Repositories;
using NineChroniclesUtilBackend.Services;
using NineChroniclesUtilBackend.Util;

namespace NineChroniclesUtilBackend.Controllers;

[ApiController]
[Route("arena")]
public class ArenaController(ArenaRankingRepository arenaRankingRepository) : ControllerBase
{
    [HttpGet("ranking/{avatarAddress}/rank")]
    public async Task<long> GetRankByAvatarAddress(string avatarAddress)
    {
        return await arenaRankingRepository.GetRankByAvatarAddress(avatarAddress);
    }

    [HttpGet("ranking")]
    public async Task<List<ArenaRanking>> GetRanking(int limit, int offset)
    {
        return await arenaRankingRepository.GetRanking(limit, offset);
    }

    [HttpPost("simulate")]
    public async Task<ArenaSimulateResponse> Simulate(
        [FromBody] ArenaSimulateRequest arenaSimulateRequest,
        IStateService stateService)
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
