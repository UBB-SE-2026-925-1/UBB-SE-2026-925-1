using Microsoft.AspNetCore.Mvc;
using MovieApp.Web.Controllers;
using FluentAssertions;
using Xunit;

namespace MovieApp.Web.Tests.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController();
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Error_ReturnsViewResult()
    {
        // Act
        var result = _controller.Error();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }
}
