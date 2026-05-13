using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieApp.Core.Interfaces.Service;
using MovieApp.Core.Services;
using MovieApp.Core.Models;
using MovieApp.Core.DTOs;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using MovieApp.WebAPI.Controllers.DTOs;
using FluentAssertions;
using Xunit;

namespace MovieApp.Web.Tests.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<IPointService> _pointServiceMock;
    private readonly Mock<IBadgeService> _badgeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _pointServiceMock = new Mock<IPointService>();
        _badgeServiceMock = new Mock<IBadgeService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new ProfileController(
            _pointServiceMock.Object,
            _badgeServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var ct = CancellationToken.None;
        var userId = 1;
        var username = "TestUser";
        
        var currentUser = new CurrentUserDTO { Id = userId, Username = username };
        _currentUserServiceMock.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _currentUserServiceMock.SetupGet(s => s.CurrentUser).Returns(currentUser);

        var stats = new UserStats { TotalPoints = 100 };
        _pointServiceMock.Setup(s => s.GetUserStatsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(stats);

        var earnedBadges = new UserBadgesDTO
        {
            UserId = userId,
            Badges = new List<BadgeDTO> { new BadgeDTO { Name = "Badge1" } }
        };
        _badgeServiceMock.Setup(s => s.GetUserBadgesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(earnedBadges);

        var allBadges = new List<Badge> { new Badge { Name = "Badge1" }, new Badge { Name = "Badge2" } };
        _badgeServiceMock.Setup(s => s.GetAllBadgesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allBadges);

        // Act
        var result = await _controller.Index(ct);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<ProfileViewModel>().Subject;
        
        model.Username.Should().Be(username);
        model.Stats.Should().BeEquivalentTo(stats);
        model.EarnedBadges.Should().HaveCount(1);
        model.EarnedBadges[0].Name.Should().Be("Badge1");
        model.AllBadges.Should().HaveCount(2);
        
        _currentUserServiceMock.Verify(s => s.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
