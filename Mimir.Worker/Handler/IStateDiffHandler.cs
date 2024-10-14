using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public interface IStateDiffHandler
{
    MimirBsonDocument ConvertToDocument(StateDiffContext context);
}
