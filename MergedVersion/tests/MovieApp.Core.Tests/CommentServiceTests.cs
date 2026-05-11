#nullable enable
using Moq;
using MovieApp.Core.Interfaces.Repository;
using MovieApp.Core.Repositories;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.Core.Tests;

public class CommentServiceAsyncTests
{
    private readonly Mock<ICommentRepository> commentRepoMock;
    private readonly Mock<IUserRepository> userRepoMock;
    private readonly Mock<IMovieRepository> movieRepoMock;
    private readonly CommentService sut;

    public CommentServiceAsyncTests()
    {
        commentRepoMock = new Mock<ICommentRepository>();
        userRepoMock = new Mock<IUserRepository>();
        movieRepoMock = new Mock<IMovieRepository>();

        sut = new CommentService(
            commentRepoMock.Object,
            userRepoMock.Object,
            movieRepoMock.Object);
    }

    // --- GetCommentsForMovieAsync ---
    [Fact]
    public async Task GetCommentsForMovieAsync_WhenCommentsExist_ReturnsCommentsOrderedByDateDescending()
    {
        var movie = new Movie { Id = 1 };
        var earlier = new Comment { MessageId = 1, MovieId = 1, Content = "First", CreatedAt = DateTime.UtcNow.AddHours(-2) };
        var later = new Comment { MessageId = 2, MovieId = 1, Content = "Second", CreatedAt = DateTime.UtcNow };
        commentRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Comment> { earlier, later });

        var result = await sut.GetCommentsForMovieAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].MessageId); // most recent first
    }

    [Fact]
    public async Task GetCommentsForMovieAsync_WhenNoCommentsForMovie_ReturnsEmptyList()
    {
        var comment = new Comment { MessageId = 1, MovieId = 99, CreatedAt = DateTime.UtcNow };
        commentRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Comment> { comment });

        var result = await sut.GetCommentsForMovieAsync(1);

        Assert.Empty(result);
    }

    // --- AddCommentAsync ---
    [Fact]
    public async Task AddCommentAsync_WhenValidInputs_ReturnsCreatedComment()
    {
        var user = new User { Id = 1, Username = "u1", AuthProvider = "p", AuthSubject = "s" };
        var movie = new Movie { Id = 1 };
        userRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);
        movieRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(movie);

        var result = await sut.AddCommentAsync(1, 1, "A valid comment");

        Assert.Equal("A valid comment", result.Content);
        Assert.Equal(0, result.ParentCommentId ?? 0);
        commentRepoMock.Verify(r => r.InsertAsync(It.IsAny<Comment>(), default), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_WhenContentExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var longContent = new string('x', 10001);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddCommentAsync(1, 1, longContent));
    }

    [Fact]
    public async Task AddCommentAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddCommentAsync(99, 1, "Hello"));
    }

    [Fact]
    public async Task AddCommentAsync_WhenMovieNotFound_ThrowsInvalidOperationException()
    {
        userRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(new User { Id = 1, Username = "u1", AuthProvider = "p", AuthSubject = "s" });
        movieRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddCommentAsync(1, 99, "Hello"));
    }

    // --- AddReplyAsync ---
    [Fact]
    public async Task AddReplyAsync_WhenParentCommentExists_ReturnsReplyWithParentSet()
    {
        var parentComment = new Comment { MessageId = 5, MovieId = 1 };
        var user = new User { Id = 1, Username = "u1", AuthProvider = "p", AuthSubject = "s" };

        commentRepoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(parentComment);
        userRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);

        var result = await sut.AddReplyAsync(1, 5, "Nice reply");

        Assert.Equal("Nice reply", result.Content);
        Assert.Equal(5, result.ParentCommentId);
        commentRepoMock.Verify(r => r.InsertAsync(It.IsAny<Comment>(), default), Times.Once);
    }

    [Fact]
    public async Task AddReplyAsync_WhenParentCommentNotFound_ThrowsInvalidOperationException()
    {
        commentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), default)).ReturnsAsync((Comment?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddReplyAsync(1, 999, "reply"));
    }

    [Fact]
    public async Task AddReplyAsync_WhenContentExceedsMaxLength_ThrowsInvalidOperationException()
    {
        var parentComment = new Comment { MessageId = 5, MovieId = 1 };
        commentRepoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(parentComment);
        var longContent = new string('x', 10001);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddReplyAsync(1, 5, longContent));
    }

    // --- DeleteCommentAsync ---
    [Fact]
    public async Task DeleteCommentAsync_CallsRepositoryDelete()
    {
        await sut.DeleteCommentAsync(10);

        commentRepoMock.Verify(r => r.DeleteAsync(10, default), Times.Once);
    }
}
