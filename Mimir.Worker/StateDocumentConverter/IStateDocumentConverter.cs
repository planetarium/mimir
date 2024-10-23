using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public interface IStateDocumentConverter
{
    MimirBsonDocument ConvertToDocument(AddressStatePair context);
}