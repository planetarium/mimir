using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public interface IStateHandler
{
    MimirBsonDocument ConvertToDocument(StateDiffContext context);
}
