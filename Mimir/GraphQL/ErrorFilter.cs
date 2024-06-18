using Mimir.Exceptions;

namespace Mimir.GraphQL;

public class ErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        var e = error.Exception;
        if (e is null)
        {
            return error;
        }

        var defaultMessage = e is IDefaultMessageException dme
            ? dme.GetDefaultMessage()
            : e.Message;
        return e.InnerException is null
            ? error.WithMessage(defaultMessage)
            : error
                .WithMessage(defaultMessage)
                .SetExtension("innerMessage", e.InnerException.Message);
    }
}
