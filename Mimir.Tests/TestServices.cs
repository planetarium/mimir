using System.Numerics;
using System.Security.Cryptography;
using HotChocolate;
using HotChocolate.Execution;
using Lib9c.GraphQL.Types;
using Lib9c.Models.States;
using Libplanet.Common;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.MongoDB.Services;
using MongoDB.Driver;
using Moq;

namespace Mimir.Tests;

public static class TestServices
{
    public static IServiceProvider CreateServices(
        Action<IServiceCollection>? configure = null,
        Mock<IMongoDbService>? mongoDbServiceMock = null,
        Mock<IActionPointRepository>? actionPointRepositoryMock = null,
        Mock<IAllCombinationSlotStateRepository>? allCombinationSlotStateRepositoryMock = null
    )
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddGraphQLServer()
            .AddLib9cGraphQLTypes()
            .AddMimirGraphQLTypes()
            .BindRuntimeType(typeof(Address), typeof(AddressType))
            .BindRuntimeType(typeof(BigInteger), typeof(BigIntegerType))
            .BindRuntimeType(typeof(HashDigest<SHA256>), typeof(HashDigestSHA256Type));

        serviceCollection.AddSingleton(
            mongoDbServiceMock?.Object ?? CreateDefaultMongoDbMock().Object
        );
        serviceCollection.AddSingleton(
            actionPointRepositoryMock?.Object ?? CreateDefaultActionPointRepositoryMock().Object
        );
        serviceCollection.AddSingleton(
            allCombinationSlotStateRepositoryMock?.Object ?? CreateDefaultAllCombinationSlotStateRepositoryMock().Object
        );
        serviceCollection.AddSingleton<ActionPointRepository>();

        configure?.Invoke(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    public static async Task<string> ExecuteRequestAsync(
        IServiceProvider serviceProvider,
        Action<IQueryRequestBuilder> configureRequest,
        CancellationToken cancellationToken = default
    )
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var requestBuilder = new QueryRequestBuilder();
        requestBuilder.SetServices(scope.ServiceProvider);
        configureRequest(requestBuilder);
        var request = requestBuilder.Create();

        var executor = scope
            .ServiceProvider.GetRequiredService<IRequestExecutorResolver>()
            .GetRequestExecutorAsync()
            .GetAwaiter()
            .GetResult();

        await using var result = await executor.ExecuteAsync(request, cancellationToken);
        result.ExpectQueryResult();
        return result.ToJson();
    }

    private static Mock<IMongoDbService> CreateDefaultMongoDbMock()
    {
        var mock = new Mock<IMongoDbService>();
        mock.Setup(m => m.GetCollection<ActionPointDocument>(It.IsAny<string>()))
            .Returns(Mock.Of<IMongoCollection<ActionPointDocument>>());
        mock.Setup(m => m.GetCollection<AllCombinationSlotStateDocument>(It.IsAny<string>()))
            .Returns(Mock.Of<IMongoCollection<AllCombinationSlotStateDocument>>());
        return mock;
    }

    private static Mock<IActionPointRepository> CreateDefaultActionPointRepositoryMock()
    {
        var mock = new Mock<IActionPointRepository>();
        mock.Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new ActionPointDocument(1, new Address(), 120));
        return mock;
    }

    private static Mock<IAllCombinationSlotStateRepository> CreateDefaultAllCombinationSlotStateRepositoryMock()
    {
        var mock = new Mock<IAllCombinationSlotStateRepository>();
        mock.Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new AllCombinationSlotStateDocument(
                1,
                new Address("0x0000000001000000000200000000030000000004"),
                new AllCombinationSlotState
                {
                    CombinationSlots = new Dictionary<int, CombinationSlotState>
                    {
                        { 0, new CombinationSlotState { Index = 0 } },
                        { 1, new CombinationSlotState { Index = 1 } },
                    }
                }));
        return mock;
    }
}
