using Libplanet.Crypto;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

public record MimirBsonDocument([property: JsonIgnore] string Id, DocumentMetadata Metadata);
