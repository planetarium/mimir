using Nekoyume.Model;
using Nekoyume.TableData;
using Nekoyume.Model.BattleStatus.Arena;
using Nekoyume.Arena;
using SimulatorRandom = NineChroniclesUtilBackend.Random;
using SystemRandom = System.Random;

namespace NineChroniclesUtilBackend.Arena;

public class ArenaBulkSimulator
{
    private readonly int _rounds;
    private const int TotalSimulations = 10;

    public ArenaBulkSimulator(int rounds = 5)
    {
        _rounds = rounds;
    }

    public async Task<double> BulkSimulate(AvatarStatesForArena myAvatar, AvatarStatesForArena enemyAvatar, ArenaSimulatorSheets simulatorSheets)
    {
        int winCount = 0;

        var tasks = Enumerable.Range(0, TotalSimulations).Select(async _ =>
        {
            var arenaLog = SimulateBattle(myAvatar, enemyAvatar, simulatorSheets);
            if (arenaLog.Result == ArenaLog.ArenaResult.Win)
            {
                Interlocked.Increment(ref winCount);
            }
        });

        await Task.WhenAll(tasks);

        return (double)winCount / TotalSimulations;
    }

    public ArenaLog SimulateBattle(AvatarStatesForArena myAvatar, AvatarStatesForArena enemyAvatar, ArenaSimulatorSheets simulatorSheets)
    {
        var seed = new SystemRandom().Next();
        var random = new SimulatorRandom(seed);
        var simulator = new ArenaSimulator(random, _rounds);
        
        var arenaLog = simulator.Simulate(
            new ArenaPlayerDigest(myAvatar.AvatarState, myAvatar.ItemSlotState.Equipments, myAvatar.ItemSlotState.Costumes, myAvatar.RuneStates),
            new ArenaPlayerDigest(enemyAvatar.AvatarState, enemyAvatar.ItemSlotState.Equipments, enemyAvatar.ItemSlotState.Costumes, enemyAvatar.RuneStates),
            simulatorSheets,
            true);
        
        return arenaLog;
    }
}