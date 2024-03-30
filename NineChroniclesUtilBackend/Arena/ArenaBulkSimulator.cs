using Nekoyume.Model;
using Nekoyume.TableData;
using Nekoyume.Model.BattleStatus.Arena;
using Nekoyume.Arena;
using Libplanet.Crypto;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using SimulatorRandom = NineChroniclesUtilBackend.Random;
using SystemRandom = System.Random;

namespace NineChroniclesUtilBackend.Arena;

public class ArenaBulkSimulator
{
    private readonly int _rounds;
    private const int TotalSimulations = 1000;

    public ArenaBulkSimulator(int rounds = 5)
    {
        _rounds = rounds;
    }

    public async Task<double> BulkSimulate(
        AvatarStatesForArena myAvatar,
        AvatarStatesForArena enemyAvatar,
        ArenaSimulatorSheets simulatorSheets,
        Dictionary<Address, CollectionState> collectionStates,
        CollectionSheet collectionSheets,
        DeBuffLimitSheet deBuffLimitSheet)
    {
        int winCount = 0;

        var tasks = Enumerable.Range(0, TotalSimulations).Select(async _ =>
        {
            var arenaLog = SimulateBattle(myAvatar, enemyAvatar, simulatorSheets, collectionStates, collectionSheets, deBuffLimitSheet);
            if (arenaLog.Result == ArenaLog.ArenaResult.Win)
            {
                Interlocked.Increment(ref winCount);
            }
        });

        await Task.WhenAll(tasks);

        return (double)winCount / TotalSimulations;
    }

    public ArenaLog SimulateBattle(
        AvatarStatesForArena myAvatar,
        AvatarStatesForArena enemyAvatar,
        ArenaSimulatorSheets simulatorSheets,
        Dictionary<Address, CollectionState> collectionStates,
        CollectionSheet collectionSheets,
        DeBuffLimitSheet deBuffLimitSheet)
    {
        var seed = new SystemRandom().Next();
        var random = new SimulatorRandom(seed);
        var simulator = new ArenaSimulator(random, _rounds);

        var modifiers = new Dictionary<Address, List<StatModifier>>
        {
            [myAvatar.AvatarState.address] = new(),
            [enemyAvatar.AvatarState.address] = new(),
        };
        if (collectionStates.Count > 0)
        {
            foreach (var (address, state) in collectionStates)
            {
                var modifier = modifiers[address];
                foreach (var collectionId in state.Ids)
                {
                    modifier.AddRange(collectionSheets[collectionId].StatModifiers);
                }
            }
        }
        
        var arenaLog = simulator.Simulate(
            new ArenaPlayerDigest(myAvatar.AvatarState, myAvatar.ItemSlotState.Equipments, myAvatar.ItemSlotState.Costumes, myAvatar.RuneStates),
            new ArenaPlayerDigest(enemyAvatar.AvatarState, enemyAvatar.ItemSlotState.Equipments, enemyAvatar.ItemSlotState.Costumes, enemyAvatar.RuneStates),
            simulatorSheets,
            modifiers[myAvatar.AvatarState.address],
            modifiers[enemyAvatar.AvatarState.address],
            deBuffLimitSheet,
            true);
        
        return arenaLog;
    }
}