using Mimir.Exceptions;

namespace Mimir.GraphQL;

public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error) =>
        error.Exception switch
        {
            DocumentNotFoundInMongoCollectionException e => error.WithMessage(e.Message),
            KeyNotFoundInBsonDocumentException e => error
                .WithMessage(e.Message)
                .SetExtension("innerMessage", e.InnerException?.Message),
            _ => error
        };
}
