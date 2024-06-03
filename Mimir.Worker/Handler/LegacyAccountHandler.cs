using System.Text.RegularExpressions;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
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
        var txs = context
            .Transactions?.Select(raw =>
                TxMarshaler.DeserializeTransactionWithoutVerification(
                    Convert.FromBase64String(raw!.SerializedPayload)
                )
            )
            .ToList();
        Dictionary<string, List<Address>> derivedAddresses =
            new Dictionary<string, List<Address>>();
        if (txs is not null)
        {
            derivedAddresses = GetDerivedFromAvatarsAddresses(txs);
        }

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
            case var addr
                when derivedAddresses.TryGetValue(
                    "ArenaScoreAddresses",
                    out var arenaScoreAddresses
                ) && arenaScoreAddresses.Contains(addr):
                return ConvertToArenaScoreState(addr, context.RawState);
            case var addr
                when derivedAddresses.TryGetValue("ArenaInfoAddresses", out var arenaInfoAddresses)
                    && arenaInfoAddresses.Contains(addr):
                return ConvertToArenaInformationState(addr, context.RawState);
            default:
                throw new InvalidOperationException("The provided address has not been handled.");
        }
    }

    private Dictionary<string, List<Address>> GetDerivedFromAvatarsAddresses(List<Transaction> txs)
    {
        var addresses = new Dictionary<string, List<Address>>
        {
            { "ArenaScoreAddresses", new List<Address>() },
            { "ArenaInfoAddresses", new List<Address>() }
        };

        foreach (var tx in txs)
        {
            var action = (Dictionary)tx.Actions[0];
            var actionType = (Text)action["type_id"];
            var actionValues = action["values"];

            if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
            {
                var avatarAddress = new Address(((Dictionary)actionValues)["maa"]);
                var enemyAvatarAddress = new Address(((Dictionary)actionValues)["eaa"]);

                for (int championshipId = 0; championshipId <= 50; championshipId++)
                {
                    for (int roundId = 0; roundId <= 7; roundId++)
                    {
                        addresses["ArenaScoreAddresses"]
                            .Add(ArenaScore.DeriveAddress(avatarAddress, championshipId, roundId));
                        addresses["ArenaInfoAddresses"]
                            .Add(
                                ArenaInformation.DeriveAddress(
                                    avatarAddress,
                                    championshipId,
                                    roundId
                                )
                            );
                        addresses["ArenaScoreAddresses"]
                            .Add(
                                ArenaScore.DeriveAddress(
                                    enemyAvatarAddress,
                                    championshipId,
                                    roundId
                                )
                            );
                        addresses["ArenaInfoAddresses"]
                            .Add(
                                ArenaInformation.DeriveAddress(
                                    enemyAvatarAddress,
                                    championshipId,
                                    roundId
                                )
                            );
                    }
                }
            }
        }

        return addresses;
    }

    private ArenaInformationState ConvertToArenaInformationState(Address address, IValue state)
    {
        if (state is List list)
        {
            return new ArenaInformationState(address, new ArenaInformation(list));
        }
        else
        {
            throw new ArgumentException(
                $"Invalid state type. Expected List. Actual: {state.GetType().Name}",
                nameof(state)
            );
        }
    }

    private ArenaScoreState ConvertToArenaScoreState(Address address, IValue state)
    {
        if (state is List list)
        {
            return new ArenaScoreState(address, new ArenaScore(list));
        }
        else
        {
            throw new ArgumentException(
                $"Invalid state type. Expected List.  Actual: {state.GetType().Name}",
                nameof(state)
            );
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
