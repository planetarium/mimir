using System.Net;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace Mimir.GraphQL;

public class HttpResponseFormatter : DefaultHttpResponseFormatter
{
    protected override HttpStatusCode OnDetermineStatusCode(
        IOperationResult result,
        FormatInfo format,
        HttpStatusCode? proposedStatusCode)
    {
        return HttpStatusCode.OK;
    }
}
