using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

public record PetStateDocument(Address Address, PetState Object) : MimirBsonDocument(Address) { }
