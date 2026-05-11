using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class ReferralLogTests
{
    [Fact]
    public void ReferralLog_PropertiesAndDefaultTimestamp_AreSetCorrectly()
    {
        var log = new ReferralLog
        {
            Id = 10,
            AmbassadorId = 5,
            ReferredUserId = 99,
            EventId = 200
        };

        Assert.Equal(10, log.Id);
        Assert.Equal(5, log.AmbassadorId);
        Assert.Equal(99, log.ReferredUserId);
        Assert.Equal(200, log.EventId);

        Assert.True((DateTime.UtcNow - log.CreatedAt).TotalSeconds < 2);
    }
}
