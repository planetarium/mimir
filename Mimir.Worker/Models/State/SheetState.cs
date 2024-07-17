using Bencodex;
using Bencodex.Types;
using Nekoyume.TableData;

namespace Mimir.Worker.Models;

public class SheetState : IBencodable
{
    private static readonly Codec Codec = new();

    public ISheet Object;

    private string RawState;

    public string Name { get; }

    public SheetState(ISheet sheet, string name, string rawState)
    {
        Object = sheet;
        Name = name;
        RawState = rawState;
    }

    public IValue Bencoded => Codec.Decode(Convert.FromHexString(RawState));
}
