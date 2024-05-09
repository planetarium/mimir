using Libplanet.Crypto;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class TableSheetData : BaseData
{
    public Address Address { get; }
    public string Name { get; }
    public ISheet Sheet { get; }
    public string Raw { get; }

    public TableSheetData(Address address, string name, ISheet sheet, string raw)
    {
        Address = address;
        Name = name;
        Sheet = sheet;
        Raw = raw;
    }
}
