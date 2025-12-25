namespace Documents.Test.Services.Categories;

public class GetByIdAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Science",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Science");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId).Returns((Category?)null);

        // Act
        Func<Task> act = async () => await _categoryService.GetByIdAsync(categoryId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found.");
    }
}
