using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public interface IStateDocumentConverter
{
    MimirBsonDocument ConvertToDocument(AddressStatePair context);
}