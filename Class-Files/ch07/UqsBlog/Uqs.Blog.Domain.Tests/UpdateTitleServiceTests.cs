using System;
using Uqs.Blog.Domain.DomainObjects;
using Uqs.Blog.Domain.Repositories;
using Uqs.Blog.Domain.Services;
using Xunit;

namespace Uqs.Blog.Domain.Tests;

public class UpdateTitleServiceTests
{
    [Fact]
    public void UpdateTitle_TitleIsNull_NormalizesToEmptyString_AndUpdatesPost()
    {
        // Arrange
        var repo = new FakePostRepository(returnPost: new Post { Id = 1, Title = "Old" });
        var sut = new UpdateTitleService(repo);

        // Act
        sut.UpdateTitle(postId: 1, title: null!);

        // Assert
        Assert.Equal(1, repo.GetByIdCallCount);
        Assert.Equal(1, repo.UpdateCallCount);
        Assert.NotNull(repo.LastUpdatedPost);
        Assert.Equal(string.Empty, repo.LastUpdatedPost!.Title);
    }

    [Fact]
    public void UpdateTitle_TrimmedBeforeUpdate()
    {
        // Arrange
        var repo = new FakePostRepository(returnPost: new Post { Id = 2, Title = "Old" });
        var sut = new UpdateTitleService(repo);

        // Act
        sut.UpdateTitle(postId: 2, title: "   Hello World   ");

        // Assert
        Assert.Equal("Hello World", repo.LastUpdatedPost!.Title);
    }

    [Fact]
    public void UpdateTitle_TitleLongerThan90_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var repo = new FakePostRepository(returnPost: new Post { Id = 3, Title = "Old" });
        var sut = new UpdateTitleService(repo);

        var tooLong = new string('A', 91);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.UpdateTitle(postId: 3, title: tooLong));

        // Assert
        Assert.Equal("title", ex.ParamName);
        Assert.Equal(0, repo.GetByIdCallCount); // length validation happens before repository access
        Assert.Equal(0, repo.UpdateCallCount);
    }

    [Fact]
    public void UpdateTitle_PostNotFound_ThrowsArgumentException()
    {
        // Arrange
        var repo = new FakePostRepository(returnPost: null);
        var sut = new UpdateTitleService(repo);

        // Act
        var ex = Assert.Throws<ArgumentException>(() => sut.UpdateTitle(postId: 404, title: "Anything"));

        // Assert
        Assert.Contains("Unable to find a post", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, repo.GetByIdCallCount);
        Assert.Equal(0, repo.UpdateCallCount);
    }

    [Fact]
    public void UpdateTitle_ValidTitle_InvokesRepositoryUpdate_WithSamePostInstance()
    {
        // Arrange
        var post = new Post { Id = 10, Title = "Old" };
        var repo = new FakePostRepository(returnPost: post);
        var sut = new UpdateTitleService(repo);

        // Act
        sut.UpdateTitle(postId: 10, title: "New Title");

        // Assert
        Assert.Same(post, repo.LastUpdatedPost);
        Assert.Equal("New Title", post.Title);
        Assert.Equal(1, repo.UpdateCallCount);
    }

    /// <summary>
    /// "Anemic model" awareness: the Post entity has no invariants for Title length/format,
    /// so the service layer is where this business rule is enforced.
    /// </summary>
    [Fact]
    public void AnemicModel_PostAllowsInvalidTitleLength_ButServiceRejectsIt()
    {
        // Arrange: entity lets us set anything (no validation)
        var post = new Post();
        post.Title = new string('B', 500);
        Assert.Equal(500, post.Title!.Length);

        // Arrange: service enforces the rule
        var repo = new FakePostRepository(returnPost: new Post { Id = 11, Title = "Old" });
        var sut = new UpdateTitleService(repo);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UpdateTitle(postId: 11, title: new string('B', 91)));
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly Post? _returnPost;

        public FakePostRepository(Post? returnPost) => _returnPost = returnPost;

        public int GetByIdCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public Post? LastUpdatedPost { get; private set; }

        public int CreatePost(int authorId) => throw new NotImplementedException();

        public Post? GetById(int postId)
        {
            GetByIdCallCount++;
            return _returnPost;
        }

        public void Update(Post post)
        {
            UpdateCallCount++;
            LastUpdatedPost = post;
        }
    }
}
