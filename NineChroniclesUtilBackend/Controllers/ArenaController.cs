using Bencodex.Types;
using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using NineChroniclesUtilBackend.Arena;
using NineChroniclesUtilBackend.Models.Arena;
using NineChroniclesUtilBackend.Repositories;
using NineChroniclesUtilBackend.Services;

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
        async Task<T> GetSheet<T>()
            where T : ISheet, new()
        {
            var sheetState = await stateService.GetState(
                Addresses.TableSheet.Derive(typeof(T).Name)
            );
            if (sheetState is not Text sheetValue)
            {
                throw new ArgumentException(nameof(T));
            }

            var sheet = new T();
            sheet.Set(sheetValue.Value);
            return sheet;
        }

        async Task<AvatarState> GetAvatarState(Address avatarAddress)
        {
            var state = await stateService.GetState(avatarAddress);
            if (state is not Dictionary dictionary)
            {
                throw new ArgumentException(nameof(avatarAddress));
            }

            var inventoryAddress = avatarAddress.Derive("inventory");
            var inventoryState = await stateService.GetState(inventoryAddress);
            if (inventoryState is not List list)
            {
                throw new ArgumentException(nameof(avatarAddress));
            }

            var inventory = new Inventory(list);

            var avatarState = new AvatarState(dictionary) { inventory = inventory };

            return avatarState;
        }

        async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
        {
            var state = await stateService.GetState(
                ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena)
            );
            return state switch
            {
                List list => new ItemSlotState(list),
                null => new ItemSlotState(BattleType.Arena),
                _ => throw new ArgumentException(nameof(avatarAddress))
            };
        }

        async Task<List<RuneState>> GetRuneStates(Address avatarAddress)
        {
            var state = await stateService.GetState(
                RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena)
            );
            var runeSlotState = state switch
            {
                List list => new RuneSlotState(list),
                null => new RuneSlotState(BattleType.Arena),
                _ => throw new ArgumentException(nameof(avatarAddress))
            };

            var runes = new List<RuneState>();
            foreach (
                var runeStateAddress in runeSlotState
                    .GetEquippedRuneSlotInfos()
                    .Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId))
            )
            {
                if (await stateService.GetState(runeStateAddress) is List list)
                {
                    runes.Add(new RuneState(list));
                }
            }

            return runes;
        }

        var myAvatarAddress = new Address(arenaSimulateRequest.MyAvatarAddress);
        var enemyAvatarAddress = new Address(arenaSimulateRequest.EnemyAvatarAddress);
        var myAvatarState = await GetAvatarState(myAvatarAddress);
        var myAvatarItemSlotState = await GetItemSlotState(myAvatarAddress);
        var myAvatarRuneStates = await GetRuneStates(myAvatarAddress);
        var enemyAvatarState = await GetAvatarState(enemyAvatarAddress);
        var enemyAvatarItemSlotState = await GetItemSlotState(enemyAvatarAddress);
        var enemyAvatarRuneStates = await GetRuneStates(enemyAvatarAddress);

        var bulkSimulator = new ArenaBulkSimulator();
        var result = await bulkSimulator.BulkSimulate(
            new AvatarStatesForArena(myAvatarState, myAvatarItemSlotState, myAvatarRuneStates),
            new AvatarStatesForArena(
                enemyAvatarState,
                enemyAvatarItemSlotState,
                enemyAvatarRuneStates
            ),
            new ArenaSimulatorSheets(
                await GetSheet<MaterialItemSheet>(),
                await GetSheet<SkillSheet>(),
                await GetSheet<SkillBuffSheet>(),
                await GetSheet<StatBuffSheet>(),
                await GetSheet<SkillActionBuffSheet>(),
                await GetSheet<ActionBuffSheet>(),
                await GetSheet<CharacterSheet>(),
                await GetSheet<CharacterLevelSheet>(),
                await GetSheet<EquipmentItemSetEffectSheet>(),
                await GetSheet<CostumeStatSheet>(),
                await GetSheet<WeeklyArenaRewardSheet>(),
                await GetSheet<RuneOptionSheet>()
            )
        );

        return new ArenaSimulateResponse(result);
    }
}
