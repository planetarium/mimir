using Libplanet.Crypto;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class TableSheetData : BaseData
{
    public Address Address { get; }
    public string Name { get; }
    public ISheet SheetJson { get; }
    public string SheetCsv { get; }
    public string Raw { get; }

    public TableSheetData(Address address, string name, ISheet sheetJson, string sheetCsv, string raw)
    {
        Address = address;
        Name = name;
        SheetJson = sheetJson;
        SheetCsv = sheetCsv;
        Raw = raw;
    }
}
