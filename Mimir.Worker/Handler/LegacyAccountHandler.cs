using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Util;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Handler;

public class LegacyAccountHandler : IStateHandler<StateData>
{
    public StateData ConvertToStateData(Address address, IValue rawState)
    {
        var state = ConvertToState(address, rawState);
        return new StateData(address, state);
    }

    public StateData ConvertToStateData(Address address, string rawState)
    {
        Codec Codec = new();
        var ivalueState = Codec.Decode(Convert.FromHexString(rawState));
        var state = ConvertToState(address, ivalueState);

        return new StateData(address, state);
    }

    private State ConvertToState(Address address, IValue rawState)
    {
        var sheetAddresses = TableSheetUtil.GetTableSheetAddresses();
        switch (address)
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

                return ConvertToTableSheetState(sheetType, addr, rawState);
            default:
                throw new InvalidOperationException("The provided address has not been handled.");
        }
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
