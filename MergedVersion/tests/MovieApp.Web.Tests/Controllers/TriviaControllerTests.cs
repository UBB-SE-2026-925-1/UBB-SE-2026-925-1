using Microsoft.AspNetCore.Mvc;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Core.DTOs;
using MovieApp.Web.Controllers;
using MovieApp.Web.Models;
using System.Threading;

namespace MovieApp.Web.Tests.Controllers;

public class TriviaControllerTests
{
    private readonly Mock<ITriviaRepository> _mockTriviaRepo;
    private readonly Mock<ITriviaRewardRepository> _mockRewardRepo;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly TriviaController _controller;

    public TriviaControllerTests()
    {
        _mockTriviaRepo = new Mock<ITriviaRepository>();
        _mockRewardRepo = new Mock<ITriviaRewardRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _mockCurrentUserService.Setup(s => s.CurrentUser).Returns(new CurrentUserDTO { Id = 1 });
        _mockCurrentUserService.Setup(s => s.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _controller = new TriviaController(_mockTriviaRepo.Object, _mockRewardRepo.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithQuestion()
    {
        // Arrange
        var category = "Actors";
        var questions = new List<TriviaQuestion> { 
            new TriviaQuestion { 
                Id = 1, 
                Category = category, 
                QuestionText = "Test?",
                OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D",
                CorrectOption = 'A'
            } 
        };
        _mockTriviaRepo.Setup(r => r.GetByCategoryAsync(category, It.IsAny<CancellationToken>())).ReturnsAsync(questions);

        // Act
        var result = await _controller.Index(category);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<TriviaIndexViewModel>().Subject;
        model.Question.Should().NotBeNull();
        model.Category.Should().Be(category);
    }

    [Fact]
    public async Task Answer_WhenCorrect_GrantsReward()
    {
        // Arrange
        var model = new TriviaAnswerInputModel { QuestionId = 1, Category = "Actors", SelectedOption = 'A' };
        var question = new TriviaQuestion { 
            Id = 1, 
            Category = "Actors", 
            CorrectOption = 'A',
            QuestionText = "Test?",
            OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D"
        };
        _mockTriviaRepo.Setup(r => r.GetByCategoryAsync("Actors", It.IsAny<CancellationToken>())).ReturnsAsync(new List<TriviaQuestion> { question });

        // Act
        var result = await _controller.Answer(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var viewModel = viewResult.Model.Should().BeOfType<TriviaIndexViewModel>().Subject;
        viewModel.IsCorrect.Should().BeTrue();
        viewModel.RewardGranted.Should().BeTrue();
        _mockRewardRepo.Verify(r => r.AddAsync(It.Is<TriviaReward>(tr => tr.UserId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Answer_WhenIncorrect_DoesNotGrantReward()
    {
        // Arrange
        var model = new TriviaAnswerInputModel { QuestionId = 1, Category = "Actors", SelectedOption = 'B' };
        var question = new TriviaQuestion { 
            Id = 1, 
            Category = "Actors", 
            CorrectOption = 'A',
            QuestionText = "Test?",
            OptionA = "A", OptionB = "B", OptionC = "C", OptionD = "D"
        };
        _mockTriviaRepo.Setup(r => r.GetByCategoryAsync("Actors", It.IsAny<CancellationToken>())).ReturnsAsync(new List<TriviaQuestion> { question });

        // Act
        var result = await _controller.Answer(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var viewModel = viewResult.Model.Should().BeOfType<TriviaIndexViewModel>().Subject;
        viewModel.IsCorrect.Should().BeFalse();
        viewModel.RewardGranted.Should().BeFalse();
        _mockRewardRepo.Verify(r => r.AddAsync(It.IsAny<TriviaReward>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
