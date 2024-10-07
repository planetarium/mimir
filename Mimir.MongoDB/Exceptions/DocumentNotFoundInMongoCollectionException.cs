namespace Mimir.MongoDB.Exceptions;

public class DocumentNotFoundInMongoCollectionException : Exception, IDefaultMessageException
{
    private const string DefaultMessage = "Document not found in MongoDB collection";

    public string GetDefaultMessage() => DefaultMessage;

    public DocumentNotFoundInMongoCollectionException(
        string collectionName,
        string filterDescription,
        Exception? innerException = null) : base(
        $"Document not found in '{collectionName}' collection with filter: {filterDescription}",
        innerException)
    {
    }
}
