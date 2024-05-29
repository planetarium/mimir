using Libplanet.Crypto;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.TableData;

namespace Mimir.Worker.Util;

public static class TableSheetUtil
{
    public static Address[] GetTableSheetAddresses()
    {
        var sheetTypes = GetTableSheetTypes();

        return sheetTypes.Select(sheet => Addresses.TableSheet.Derive(sheet.Name)).ToArray();
    }

    public static IEnumerable<Type> GetTableSheetTypes()
    {
        var sheetTypes = typeof(ISheet)
            .Assembly.GetTypes()
            .Where(type =>
                type.Namespace is { } @namespace
                && @namespace.StartsWith($"{nameof(Nekoyume)}.{nameof(Nekoyume.TableData)}")
                && !type.IsAbstract
                && typeof(ISheet).IsAssignableFrom(type)
            );

        return sheetTypes;
    }
}
