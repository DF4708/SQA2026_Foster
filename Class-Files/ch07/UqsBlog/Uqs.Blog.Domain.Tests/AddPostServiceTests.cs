using System;
using Uqs.Blog.Domain.DomainObjects;
using Uqs.Blog.Domain.Repositories;
using Uqs.Blog.Domain.Services;
using Xunit;

namespace Uqs.Blog.Domain.Tests;

public class AddPostServiceTests
{
    [Fact]
    public void AddPost_AuthorNotFound_ThrowsArgumentException()
    {
        // Arrange
        var postRepo = new FakePostRepository();
        var authorRepo = new FakeAuthorRepository(returnAuthor: null);
        var sut = new AddPostService(postRepo, authorRepo);

        // Act
        var ex = Assert.Throws<ArgumentException>(() => sut.AddPost(authorId: 123));

        // Assert
        Assert.Equal("authorId", ex.ParamName);
        Assert.Contains("Author Id not found", ex.Message);
    }

    [Fact]
    public void AddPost_AuthorLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var postRepo = new FakePostRepository();
        var authorRepo = new FakeAuthorRepository(new Author { Id = 7, IsLocked = true });
        var sut = new AddPostService(postRepo, authorRepo);

        // Act + Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sut.AddPost(authorId: 7));
        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, postRepo.CreatePostCallCount);
    }

    [Fact]
    public void AddPost_ValidAuthor_ReturnsNewPostId_AndCallsRepository()
    {
        // Arrange
        var postRepo = new FakePostRepository { CreatePostReturnValue = 456 };
        var authorRepo = new FakeAuthorRepository(new Author { Id = 9, IsLocked = false });
        var sut = new AddPostService(postRepo, authorRepo);

        // Act
        var newId = sut.AddPost(authorId: 9);

        // Assert
        Assert.Equal(456, newId);
        Assert.Equal(1, postRepo.CreatePostCallCount);
        Assert.Equal(9, postRepo.LastCreatePostAuthorId);
    }

    private sealed class FakeAuthorRepository : IAuthorRepository
    {
        private readonly Author? _returnAuthor;

        public FakeAuthorRepository(Author? returnAuthor) => _returnAuthor = returnAuthor;

        public Author? GetById(int id) => _returnAuthor;
    }

    private sealed class FakePostRepository : IPostRepository
    {
        public int CreatePostReturnValue { get; set; } = 0;

        public int CreatePostCallCount { get; private set; }
        public int? LastCreatePostAuthorId { get; private set; }

        public int CreatePost(int authorId)
        {
            CreatePostCallCount++;
            LastCreatePostAuthorId = authorId;
            return CreatePostReturnValue;
        }

        public Post? GetById(int postId) => throw new NotImplementedException();
        public void Update(Post post) => throw new NotImplementedException();
    }
}
