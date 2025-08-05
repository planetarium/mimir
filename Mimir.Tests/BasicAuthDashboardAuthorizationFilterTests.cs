using System;
using System.Text;
using Microsoft.Extensions.Options;
using Mimir.GraphQL;
using Mimir.Options;
using Xunit;

namespace Mimir.Tests;

public class BasicAuthDashboardAuthorizationFilterTests
{
    private readonly BasicAuthDashboardAuthorizationFilter _filter;

    public BasicAuthDashboardAuthorizationFilterTests()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new HangfireOption
        {
            Username = "testuser",
            Password = "testpass"
        });

        _filter = new BasicAuthDashboardAuthorizationFilter(options);
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        Assert.NotNull(_filter);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BasicAuthDashboardAuthorizationFilter(null!));
    }
} 