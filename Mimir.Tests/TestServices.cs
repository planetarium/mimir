using System.Numerics;
using System.Security.Cryptography;
using HotChocolate;
using HotChocolate.Execution;
using Lib9c.GraphQL.Types;
using Libplanet.Common;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;

namespace Mimir.Tests;

public static class TestServices
{
    public static readonly IServiceProvider Services;
    public static readonly RequestExecutorProxy Executor;

    static TestServices()
    {
        Services = new ServiceCollection()
            .AddGraphQLServer()
            .AddLib9cGraphQLTypes()
            .AddMimirGraphQLTypes()
            .BindRuntimeType(typeof(Address), typeof(AddressType))
            .BindRuntimeType(typeof(BigInteger), typeof(BigIntegerType))
            .BindRuntimeType(typeof(HashDigest<SHA256>), typeof(HashDigestSHA256Type))
            .Services
            .AddSingleton(sp => new RequestExecutorProxy(
                sp.GetRequiredService<IRequestExecutorResolver>(),
                Schema.DefaultName))
            .BuildServiceProvider();
        Executor = Services.GetRequiredService<RequestExecutorProxy>();
    }

    public static async Task<string> ExecuteRequestAsync(
        Action<IQueryRequestBuilder> configureRequest,
        CancellationToken cancellationToken = default)
    {
        await using var scope = Services.CreateAsyncScope();

        var requestBuilder = new QueryRequestBuilder();
        requestBuilder.SetServices(scope.ServiceProvider);
        configureRequest(requestBuilder);
        var request = requestBuilder.Create();

        await using var result = await Executor.ExecuteAsync(request, cancellationToken);
        result.ExpectQueryResult();
        return result.ToJson();
    }
}
