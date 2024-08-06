using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record AvatarDocument(Address Address, AvatarState Object) : MimirBsonDocument(Address);
