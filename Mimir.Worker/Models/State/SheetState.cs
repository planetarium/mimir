using Libplanet.Crypto;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class SheetState : State
{
    public ISheet Sheet;

    public SheetState(Address address, ISheet sheet)
        : base(address)
    {
        Sheet = sheet;
    }
}
