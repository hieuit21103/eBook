using System.Linq.Expressions;

namespace Documents.Test.Services.Categories;

public class UpdateAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenValidInput()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var request = new CategoryUpdateRequest { Name = "New Name" };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(new List<Category>());

        // Act
        var result = await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        existingCategory.UpdatedAt.Should().BeAfter(existingCategory.CreatedAt);
        await _categoryRepository.Received(1).UpdateAsync(existingCategory);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CategoryUpdateRequest { Name = "New Name" };

        _categoryRepository.GetByIdAsync(categoryId).Returns((Category?)null);

        // Act
        Func<Task> act = async () => await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {categoryId} not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenCategoryNameExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category { Id = categoryId, Name = "Old Name" };
        var request = new CategoryUpdateRequest { Name = "Existing Name" };

        var conflictingCategory = new Category { Id = Guid.NewGuid(), Name = "Existing Name" };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _categoryRepository.FindAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(new List<Category> { conflictingCategory });

        // Act
        Func<Task> act = async () => await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category with name 'Existing Name' already exists.");
    }
}
