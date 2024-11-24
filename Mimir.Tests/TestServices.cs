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
        Mock<IAgentRepository>? agentRepositoryMock = null,
        Mock<IAvatarRepository>? avatarRepositoryMock = null,
        Mock<IAllCombinationSlotStateRepository>? allCombinationSlotStateRepositoryMock = null,
        Mock<IDailyRewardRepository>? dailyRewardRepositoryMock = null
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
            agentRepositoryMock?.Object ?? CreateDefaultAgentRepositoryMock().Object
        );
        serviceCollection.AddSingleton(
            allCombinationSlotStateRepositoryMock?.Object ?? CreateDefaultAllCombinationSlotStateRepositoryMock().Object
        );
        
        serviceCollection.AddSingleton(
            dailyRewardRepositoryMock?.Object ?? CreateDefaultDailyRewardRepositoryMock().Object
        );

        serviceCollection.AddSingleton<ActionPointRepository>();

        serviceCollection.AddSingleton<AgentRepository>();
        
        serviceCollection.AddSingleton<DailyRewardDocument>();

        if (avatarRepositoryMock is not null)
        {
            serviceCollection.AddSingleton<IAvatarRepository>(avatarRepositoryMock.Object);
        }

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
        mock.Setup(m => m.GetCollection<AgentDocument>(It.IsAny<string>()))
            .Returns(Mock.Of<IMongoCollection<AgentDocument>>());
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

    private static Mock<IAgentRepository> CreateDefaultAgentRepositoryMock()
    {
        var agentAddress = new Address("0x0000000031000000000200000000030000000004");
        var avatarAddress1 = new Address("0x0000005001000000000200000000030000000004");
        var avatarAddress2 = new Address("0x0000008001000000000200000000030000000004");
        var avatarAddress3 = new Address("0x0000001001000000000200000000030000000004");
        var agentState = new AgentState
        {
            Address = agentAddress,
            AvatarAddresses = new Dictionary<int, Address>
            {
                { 0, avatarAddress1 },
                { 1, avatarAddress2 },
                { 2, avatarAddress3 }
            },
            MonsterCollectionRound = 3,
            Version = 4,
        };
        var mockRepo = new Mock<IAgentRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new AgentDocument(1, agentAddress, agentState));

        return mockRepo;
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
    
    private static Mock<IDailyRewardRepository> CreateDefaultDailyRewardRepositoryMock()
    {
        var mockAddress = new Address("0x0000000000000000000000000000000000000000");

        var mockRepo = new Mock<IDailyRewardRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new DailyRewardDocument(0, mockAddress, 0));

        return mockRepo;
    }
    
}