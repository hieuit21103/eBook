namespace Documents.Test.Services.Categories;

public class DeleteAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory_WhenExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Science" };

        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        // Act
        await _categoryService.DeleteAsync(categoryId);

        // Assert
        await _categoryRepository.Received(1).DeleteAsync(category);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId).Returns((Category?)null);

        // Act
        Func<Task> act = async () => await _categoryService.DeleteAsync(categoryId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found.");
    }
}
