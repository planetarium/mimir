using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class ActionPointTests
{
    [Fact]
    public async Task GetActionPoint_Returns_CorrectValue()
    {
        var actionPointMock = new Mock<IActionPointRepository>();
        actionPointMock
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new ActionPointDocument(1, new Address(), 120));

        var serviceProvider = new TestServices.ServiceProviderBuilder()
            .With(actionPointMock.Object)
            .Build();
        const string query = "query { actionPoint(address: \"0x0000000000000000000000000000000000000000\") }";
        var result = await TestServices.ExecuteRequestAsync(
            serviceProvider,
            b => b.SetQuery(query));

        await Verify(result);
    }
}
