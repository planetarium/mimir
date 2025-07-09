using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lib9c.Models.Block;

[JsonConverter(typeof(StringEnumConverter))]
public enum TxStatus
{
    INVALID,
    STAGING,
    SUCCESS,
    FAILURE,
    INCLUDED,
}
