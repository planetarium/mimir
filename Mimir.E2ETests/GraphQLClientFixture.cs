using Microsoft.Extensions.DependencyInjection;

namespace Mimir.E2ETests;

public class GraphQLClientFixture
{
    public IServiceProvider ServiceProvider { get; private set; }

    public GraphQLClientFixture()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddMimirClient()
            .ConfigureHttpClient(client =>
                client.BaseAddress = new Uri("https://mimir.nine-chronicles.dev/odin/graphql/")
            );

        serviceCollection
            .AddHeadlessClient()
            .ConfigureHttpClient(client =>
                client.BaseAddress = new Uri("https://9c-main-rpc-1.nine-chronicles.com/graphql")
            );

        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}
