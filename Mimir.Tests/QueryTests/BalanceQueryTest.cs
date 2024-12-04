using HotChocolate.AspNetCore;
using Lib9c.GraphQL.Extensions;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;

namespace Mimir.Tests.QueryTests;

public class BalanceQueryTest
{
    [Fact]
    public async Task GraphQL_Query_Balance_CRYSTAL_Returns_CorrectValue()
    {
        var mockAddress = new Address("0x0000000000000000000000000000000000000000");
        var mockRepo = new Mock<IBalanceRepository>();

        mockRepo
            .Setup(repo => repo.GetByAddressAsync("CRYSTAL".ToCurrency(), mockAddress))
            .ReturnsAsync(new BalanceDocument(1, new Address(), "fgfgf"));
        
        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                          query {
                            balance(currencyTicker: "CRYSTAL", address: "{{mockAddress}}")
                          }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }

    [Fact]
    public async Task GraphQL_Query_Balance_NCG_Returns_CorrectValue()
    {
        var mockAddress = new Address("0x0000000000000000000000000000000000000000");
        var mockRepo = new Mock<IBalanceRepository>();

        mockRepo
            .Setup(repo => repo.GetByAddressAsync("NCG".ToCurrency(), mockAddress))
            .ReturnsAsync(new BalanceDocument(1, new Address(), "fgfgf"));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                          query {
                            balance(currencyTicker: "NCG", address: "{{mockAddress}}")
                          }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
    
    [Fact]
    public async Task GraphQL_Query_Balance_Throws_When_No_Inputs_Provided()
    {
        var mockRepo = new Mock<IBalanceRepository>();
        
        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        var query = $$"""
                          query {
                            balance(address: "0x0000000000000000000000000000000000000000")
                          }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));
        await Verify(result);
    }
}