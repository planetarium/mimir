using Libplanet.Crypto;
using Newtonsoft.Json;

namespace Mimir.MongoDB.Bson;

public record MimirBsonDocument([property: JsonIgnore] Address Id, DocumentMetadata Metadata);
