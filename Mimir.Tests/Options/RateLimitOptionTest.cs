using Mimir.Options;
using Xunit;

namespace Mimir.Tests.Options;

public class RateLimitOptionTest
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var option = new RateLimitOption();

        Assert.True(option.IsEnabled);
        Assert.Equal(50, option.PermitLimit);
        Assert.Equal(10, option.Window);
        Assert.Equal(10, option.ReplenishmentPeriod);
        Assert.Equal(2, option.QueueLimit);
        Assert.Equal(8, option.SegmentsPerWindow);
        Assert.Equal(50, option.TokenLimit);
        Assert.Equal(50, option.TokensPerPeriod);
        Assert.True(option.AutoReplenishment);
    }

    [Fact]
    public void IsEnabled_WhenSetToFalse_ShouldBeFalse()
    {
        var option = new RateLimitOption
        {
            IsEnabled = false
        };

        Assert.False(option.IsEnabled);
    }

    [Fact]
    public void IsEnabled_WhenSetToTrue_ShouldBeTrue()
    {
        var option = new RateLimitOption
        {
            IsEnabled = true
        };

        Assert.True(option.IsEnabled);
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        Assert.Equal("RateLimit", RateLimitOption.SectionName);
    }
}

