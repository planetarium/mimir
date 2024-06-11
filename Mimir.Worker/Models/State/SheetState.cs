using Libplanet.Crypto;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class SheetState : State
{
    public ISheet Object;

    public string Name { get; }

    public SheetState(Address address, ISheet sheet, string name)
        : base(address)
    {
        Object = sheet;
        Name = name;
    }
}
