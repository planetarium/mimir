using Libplanet.Crypto;

namespace Mimir.Tests.QueryTests;

public class ActionPointTests
{
    [Fact]
    public async Task GetActionPoint_Returns_CorrectValue()
    {
        // var query =
        //     @"
        // query($address: Address!) {
        //     actionPoint(address: $address)
        // }";
        var query = "query { actionPoint(address: \"0x0000000000000000000000000000000000000000\")}";
        var result = await TestServices.ExecuteRequestAsync(b => b.SetQuery(query));

        await Verify(result);
    }
}
