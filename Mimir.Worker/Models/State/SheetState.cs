using Libplanet.Crypto;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class SheetState : State
{
    public ISheet Object;

    public SheetState(Address address, ISheet sheet)
        : base(address)
    {
        Object = sheet;
    }
}
