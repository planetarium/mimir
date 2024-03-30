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
        IStateService stateService
    )
    {
        var stateGetter = new StateGetter(stateService);

        var myAvatarAddress = new Address(arenaSimulateRequest.MyAvatarAddress);
        var enemyAvatarAddress = new Address(arenaSimulateRequest.EnemyAvatarAddress);
        var myAvatarState = await stateGetter.GetAvatarState(myAvatarAddress);
        var myAvatarItemSlotState = await stateGetter.GetItemSlotState(myAvatarAddress);
        var myAvatarRuneStates = await stateGetter.GetRuneStates(myAvatarAddress);
        var enemyAvatarState = await stateGetter.GetAvatarState(enemyAvatarAddress);
        var enemyAvatarItemSlotState = await stateGetter.GetItemSlotState(enemyAvatarAddress);
        var enemyAvatarRuneStates = await stateGetter.GetRuneStates(enemyAvatarAddress);

        var bulkSimulator = new ArenaBulkSimulator();
        var result = await bulkSimulator.BulkSimulate(
            new AvatarStatesForArena(myAvatarState, myAvatarItemSlotState, myAvatarRuneStates),
            new AvatarStatesForArena(
                enemyAvatarState,
                enemyAvatarItemSlotState,
                enemyAvatarRuneStates
            ),
            new ArenaSimulatorSheets(
                await stateGetter.GetSheet<MaterialItemSheet>(),
                await stateGetter.GetSheet<SkillSheet>(),
                await stateGetter.GetSheet<SkillBuffSheet>(),
                await stateGetter.GetSheet<StatBuffSheet>(),
                await stateGetter.GetSheet<SkillActionBuffSheet>(),
                await stateGetter.GetSheet<ActionBuffSheet>(),
                await stateGetter.GetSheet<CharacterSheet>(),
                await stateGetter.GetSheet<CharacterLevelSheet>(),
                await stateGetter.GetSheet<EquipmentItemSetEffectSheet>(),
                await stateGetter.GetSheet<CostumeStatSheet>(),
                await stateGetter.GetSheet<WeeklyArenaRewardSheet>(),
                await stateGetter.GetSheet<RuneOptionSheet>()
            ),
            await stateGetter.GetCollectionStates([myAvatarAddress, enemyAvatarAddress]),
            await stateGetter.GetSheet<CollectionSheet>(),
            await stateGetter.GetSheet<DeBuffLimitSheet>()
        );

        return new ArenaSimulateResponse(result);
    }
}
