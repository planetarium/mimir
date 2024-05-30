using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Util;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.Arena;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Handler;

public class LegacyAccountHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(StateDiffContext context)
    {
        var state = ConvertToState(context);
        return new StateData(context.Address, state);
    }

    private State ConvertToState(StateDiffContext context)
    {
        var sheetAddresses = TableSheetUtil.GetTableSheetAddresses();
        switch (context.Address)
        {
            case var addr when sheetAddresses.Contains(addr):
                var sheetTypes = TableSheetUtil.GetTableSheetTypes();
                var sheetType = sheetTypes
                    .Where(sheet => Addresses.TableSheet.Derive(sheet.Name) == addr)
                    .FirstOrDefault();

                if (sheetType == null)
                {
                    throw new TypeLoadException(
                        $"Unable to find a class type matching the address '{addr}' in the specified namespace."
                    );
                }

                return ConvertToTableSheetState(sheetType, addr, context.RawState);
            default:
                throw new InvalidOperationException("The provided address has not been handled.");
        }
    }

    private ArenaScoreState ConvertToArenaScoreState(Address address, IValue state)
    {
        return new ArenaScoreState(address, new ArenaScore(address, 1, 1, 1));
    }

    private SheetState ConvertToTableSheetState(Type sheetType, Address address, IValue state)
    {
        if (state is not Text sheetValue)
        {
            throw new ArgumentException(nameof(sheetType));
        }

        if (sheetType == typeof(ItemSheet) || sheetType == typeof(QuestSheet))
        {
            throw new ArgumentException(
                $"{nameof(ItemSheet)} and {nameof(QuestSheet)} is not table sheet"
            );
        }

        if (
            sheetType == typeof(WorldBossKillRewardSheet)
            || sheetType == typeof(WorldBossBattleRewardSheet)
        )
        {
            throw new NotImplementedException(
                "Handling for WorldBossKillRewardSheet and WorldBossBattleRewardSheet is not implemented yet."
            );
        }

        var sheetInstance = Activator.CreateInstance(sheetType);
        if (sheetInstance is not ISheet sheet)
        {
            throw new InvalidCastException($"Type {sheetType.Name} cannot be cast to ISheet.");
        }

        sheet.Set(sheetValue.Value);

        return new SheetState(address, sheet);
    }
}
