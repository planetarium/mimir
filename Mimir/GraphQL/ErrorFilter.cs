using Mimir.Exceptions;

namespace Mimir.GraphQL;

public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error) =>
        error.Exception switch
        {
            DocumentNotFoundInMongoCollectionException => error
                .WithMessage(DocumentNotFoundInMongoCollectionException.DefaultMessage),
            KeyNotFoundInBsonDocumentException e => error
                .WithMessage(e.Message)
                .SetExtension("innerMessage", e.InnerException?.Message),
            UnexpectedTypeOfBsonValueException e => error.WithMessage(e.Message),
            _ => error
        };
}
