using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Documents.Test.Services.Documents;

public class CreateAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task CreateAsync_ShouldCreateDocument_WhenValidInput()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Science" };
        var userId = Guid.NewGuid();

        var request = new DocumentCreateRequest
        {
            Title = "Test Document",
            Topic = "Test Topic",
            CategoryIds = new List<Guid> { category.Id }
        };

        var savedDocument = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Topic = request.Topic,
            TotalPages = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Categories = new List<DocumentCategory>
            {
                new DocumentCategory
                {
                    CategoryId = category.Id,
                    Category = category
                }
            }
        };

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);

        _documentRepository.AddAsync(Arg.Is<Document>(d =>
                d.Title == request.Title &&
                d.Topic == request.Topic &&
                d.UserId == userId
            ))
            .Returns(savedDocument);

        var expectedResponse = new DocumentResponse
        {
            Id = savedDocument.Id,
            UserId = savedDocument.UserId,
            Title = savedDocument.Title,
            Topic = savedDocument.Topic,
            CreatedAt = savedDocument.CreatedAt,
            UpdatedAt = savedDocument.UpdatedAt,
            Categories = new List<string> { category.Name },
        };

        _documentRepository.GetByIdWithDetailsAsync(savedDocument.Id).Returns(savedDocument);

        // Act
        var result = await _documentService.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();

        result.Should().BeEquivalentTo(expectedResponse);

        await _documentRepository.Received(1).AddAsync(Arg.Any<Document>());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidCategoryId = Guid.NewGuid();

        var request = new DocumentCreateRequest
        {
            Title = "Test Document",
            Topic = "Test Topic",
            CategoryIds = new List<Guid> { invalidCategoryId }
        };

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(false);

        // Act
        Func<Task> act = async () => await _documentService.CreateAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {invalidCategoryId} not found.");
    }
}
