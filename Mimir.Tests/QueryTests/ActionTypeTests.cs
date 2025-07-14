using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class ActionTypeTests
{
    [Fact]
    public async Task GetActionTypes_Returns_CorrectValue()
    {
        var actionTypeMock = new Mock<IActionTypeRepository>();
        var actionTypes = new List<ActionTypeDocument>
        {
            new ActionTypeDocument("action1"),
            new ActionTypeDocument("action2"),
            new ActionTypeDocument("action3")
        };
        actionTypeMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(actionTypes);

        var serviceProvider = TestServices.Builder
            .With(actionTypeMock.Object)
            .Build();
        const string query = "query { actionTypes { id } }";
        var result = await TestServices.ExecuteRequestAsync(
            serviceProvider,
            b => b.SetDocument(query));

        await Verify(result);
    }
} 