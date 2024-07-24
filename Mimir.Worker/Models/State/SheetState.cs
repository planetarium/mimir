using Bencodex;
using Bencodex.Types;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class SheetState : IBencodable
{
    private static readonly Codec Codec = new();

    public ISheet Object;

    private IValue _rawState;

    public string Name { get; }

    public SheetState(ISheet sheet, string name, IValue rawState)
    {
        Object = sheet;
        Name = name;
        _rawState = rawState;
    }

    public IValue Bencoded => _rawState;
}
